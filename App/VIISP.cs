
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace App.VIISP;

/// <summary>Vartotojo autorizacijos modelis</summary>
public class Auth {
	private static HttpClient HClient { get; set; }
	private static DateTime NextClean { get; set; } = DateTime.UtcNow;
	private static ConcurrentDictionary<string, AuthLock> LockList { get; set; } = new();
	private static ConcurrentDictionary<Guid, AuthRequest> Redirect { get; set; } = new();

	/// <summary>DDOS apsauga nuo perdidelio kiekio užklausų iš vieno IP</summary>
	/// <param name="req">Vartotojo užklausa</param>
	private static void LockIP(HttpContext req) {
		//TODO: Find correct IP (unbalanced);
		var ip = req.Connection.RemoteIpAddress?.ToString();
		if(!string.IsNullOrEmpty(ip)){
			if(!LockList.TryGetValue(ip, out var lck)){ lck = new(); LockList.TryAdd(ip, lck); }
			lock(lck){ lck.LastLock=DateTime.UtcNow; lck.Count++; var dly = Config.GetLong("Auth", "LockDelay", 0); if(dly>0) Thread.Sleep((int)dly); }
			if(NextClean<DateTime.UtcNow){
				NextClean = DateTime.UtcNow.AddSeconds(Config.GetLong("Auth", "LockCleanInterval", 300));
				var cleanint = DateTime.UtcNow.AddSeconds(Config.GetLong("Auth", "LockCleanDelay", 300));
				var clean = new List<string>();
				foreach(var i in LockList) if(i.Value.LastLock < cleanint) clean.Add(i.Key);
			
				if(clean.Count>0){
					var report = Config.GetLong("Auth","LockReport",10);
					foreach(var i in clean)  {
						if(LockList.TryRemove(i, out var itm)){
							if(itm.Count>=report) { /* Log multiple logins from same IP */ } //TODO: Report here
						}
					}
				}
				var cleanr = new List<Guid>();
				foreach(var i in Redirect) if(i.Value.Timeout < DateTime.UtcNow) cleanr.Add(i.Key);
				foreach(var ri in cleanr) Redirect.TryRemove(ri, out _);
			}
		} else {
			//TODO: throw something;
		}
	}

	static Auth() {
		HClient = new(){
			Timeout = new TimeSpan(0,0,Config.GetInt("Auth", "Timeout", 15)), 
			BaseAddress = new Uri($"https://{Config.GetVal("Auth", "Host")}/")
		};
		HClient.DefaultRequestHeaders.Add("X-Api-Key", Config.GetVal("Auth", "Token"));
	}


	/// <summary>Vartotojo autorizacijos iniciavimas</summary>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	public static async Task<AuthRequest?> GetAuth(HttpContext ctx, CancellationToken ct){
		LockIP(ctx);
		if(!ct.IsCancellationRequested) {
			var msg = new StringContent($"{{\"host\":\"{Config.GetVal("Auth","Redirect","http://localhost:5000/api/login")}\"}}", new MediaTypeHeaderValue("application/json"));			
			try {
				using var response = await HClient.PostAsync(Config.GetVal("Auth","GetSignin","/auth/evartai/sign"), msg, ct);
				if(response.IsSuccessStatusCode){
					using var rsp = await response.Content.ReadAsStreamAsync(ct);
					var tck = JsonSerializer.Deserialize<AuthTicket>(rsp);					
					if(tck?.Ticket is not null){
						var ath = new AuthRequest((Guid)tck.Ticket) { Return = ctx.Request.Query.TryGetValue("r", out var r)?r:""};
						Redirect.TryAdd(ath.ID,ath);
						ctx.Response.Redirect(tck.Url??"/");
						return ath;
					}
					else { Console.WriteLine("Fail: " + rsp); }
				} else { 
					var rsp = await response.Content.ReadAsStringAsync(ct);
					Console.WriteLine("Fail: " + rsp); 
				}
			}
			catch (Exception ex) { Console.WriteLine("Fail: " + ex.Message); }
		}
		return null;
	}

	/// <summary>Vartotojo autorizacijos tikrinimas</summary>
	/// <param name="ticket">Autorizacijos kodas</param>
	/// <param name="ct"></param>
	public static async Task<AuthUser?> GetUser(Guid ticket, CancellationToken ct){
		if(Redirect.TryRemove(ticket, out var tck)){
			if(tck.Timeout>DateTime.UtcNow){
				var m = new StringContent($"{{\"ticket\":\"{ticket}\",\"defaultGroupId\":null,\"refresh\":false}}",new MediaTypeHeaderValue("application/json"));			
    			
				try {
					using var response = await HClient.PostAsync(Config.GetVal("Auth","GetLogin","/auth/evartai/login"), m, ct);
					if(response.IsSuccessStatusCode){
						using var rsp = await response.Content.ReadAsStreamAsync(ct);
						var tkn = JsonSerializer.Deserialize<AuthToken>(rsp);						
						if(!string.IsNullOrEmpty(tkn?.Token)){
							return await GetUserDetails(tkn.Token,ct);
						}
						else { Console.WriteLine("Fail: " + rsp); }
					} else { 
						var rsp = await response.Content.ReadAsStringAsync(ct);
						Console.WriteLine("Fail: " + rsp); 
					}
				}
				catch (Exception ex) { Console.WriteLine("Fail: " + ex.Message); }
			} else { Console.WriteLine("Fail: Timeout "+ tck.Timeout); }
		} else { Console.WriteLine("Fail: " + ticket); }
		return null;
	}

	/// <summary>Vartotojo autorizacijos detalės</summary>
	/// <param name="token">Vartotojo autorizacijos raktas</param>
	/// <param name="ct"></param>
	public static async Task<AuthUser?> GetUserDetails(string token,CancellationToken ct){
		using var msg = new HttpRequestMessage(HttpMethod.Post, Config.GetVal("Auth","GetUser","/api/users/me"));
		msg.Headers.Authorization = new("Bearer",token);
		try {
			using var response = await HClient.SendAsync(msg,ct);		
			if(response.IsSuccessStatusCode){
				using var rsp = await response.Content.ReadAsStreamAsync(ct);	
				var usr = JsonSerializer.Deserialize<AuthUser>(rsp);						
				Console.WriteLine("User: " + rsp);
				
				if(!string.IsNullOrEmpty(usr?.AK)){							
					return usr;
				}
				else { Console.WriteLine("Fail: " + rsp); }
			} else { 
				var rsp = await response.Content.ReadAsStringAsync(ct);
				Console.WriteLine("Fail: " + rsp); 
			}
		}
		catch (Exception ex) { Console.WriteLine("Fail: " + ex.Message); }
		return null;
	}

}

/// <summary>Autorizacijos apsauga</summary>
public class AuthLock {
	/// <summary>Pradinis autorizacijos laikas</summary>
	public DateTime Start { get; set; } = DateTime.UtcNow;
	/// <summary>Paskutinės Autorizacijos laikas</summary>
	public DateTime LastLock { get; set; } = DateTime.UtcNow;
	/// <summary>Autorizacijos kiekis</summary>
	public long Count { get; set; } = 0;
}

/// <summary>Autorizacijos užklausa</summary>
public class AuthRequest{
	/// <summary>Autorizacijos identifikavimo numeris</summary>
	public Guid ID { get; set; }
	/// <summary>Vartotojo peradresavimas po autorizacijos</summary>
	public string? Return { get; set; }
	/// <summary>Vartotojo autorizacijos laiko limitas</summary>
	public DateTime Timeout { get; set; }
	
	/// <summary>Užklausos konstruktorius</summary>
	/// <param name="ticket">Autorizacijos identifikavimo numeris</param>
	public AuthRequest(Guid ticket){
		ID=ticket;
		Timeout=DateTime.UtcNow.AddSeconds(Config.GetLong("Auth", "Timeout", 300));
	}
}


/// <summary>BIIP Vartotojo detalės</summary>
public class AuthUser{
	/// <summary>BIIP ID</summary>
	[JsonPropertyName("id")] public int ID { get; set; }
	/// <summary>AK</summary>
	[JsonPropertyName("personalCode")] public string? AK { get; set; }
	/// <summary>Vardas</summary>
	[JsonPropertyName("firstName")] public string? FName { get; set; }
	/// <summary>Pavardė</summary>
	[JsonPropertyName("lastName")] public string? LName { get; set; }
	/// <summary>El. Paštas</summary>
	[JsonPropertyName("email")] public string? Email { get; set; }
	/// <summary>Tel. Nr.</summary>
	[JsonPropertyName("phone")] public string? Phone { get; set; }
	/// <summary>Vartotojo tipas</summary>
	[JsonPropertyName("type")] public string? Type { get; set; }
	/// <summary>Pilnas vardas</summary>
	[JsonPropertyName("fullName")] public string? Name { get; set; }
}

/// <summary>BIIP Autorizacija</summary>
public class AuthTicket{
	/// <summary>AUtorizacijos kodas</summary>
	[JsonPropertyName("ticket")] public Guid? Ticket { get; set; }
	/// <summary>VIISP adresas</summary>
	[JsonPropertyName("host")] public string? Host { get; set; }
	/// <summary>VIISP peradresavimas</summary>
	[JsonPropertyName("url")] public string? Url { get; set; }
}

/// <summary>BIIP Vartotojo autorizacija</summary>
public class AuthToken{
	/// <summary>Prisijungitmo raktas</summary>
	[JsonPropertyName("token")] public string? Token { get; set; }
}