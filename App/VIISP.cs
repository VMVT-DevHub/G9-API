using App.Users;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
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
	/// <param name="ctx">Vartotojo užklausa</param>
	private static void LockIP(HttpContext ctx) {
		//TODO: Find correct IP (unbalanced);
		var ip = ctx.GetIP();
		if(!string.IsNullOrEmpty(ip)){
			if(!LockList.TryGetValue(ip, out var lck)){ lck = new(); LockList.TryAdd(ip, lck); }
			lock(lck){ lck.LastLock=DateTime.UtcNow; lck.Count++; var dly = Config.GetLong("Auth", "LockDelay", 1); if(dly>0) Thread.Sleep((int)dly); }
			if(NextClean<DateTime.UtcNow){
				NextClean = DateTime.UtcNow.AddSeconds(Config.GetLong("Auth", "LockCleanInterval", 300));
				var cleanint = DateTime.UtcNow.AddSeconds(Config.GetLong("Auth", "LockCleanDelay", 300));
				var clean = new List<string>();
				foreach(var i in LockList) if(i.Value.LastLock < cleanint) clean.Add(i.Key);			
				if(clean.Count>0){
					var report = Config.GetLong("Auth","LockReport",10);
					foreach(var i in clean)  {
						if(LockList.TryRemove(i, out var itm)){
							if(itm.Count>=report) { 
								new DBExec("INSERT INTO app.log_error (log_code,log_msg,log_data,log_ip) VALUES (1019,'Too many logins',@data,@ip);",
									("@data",JsonSerializer.Serialize(itm)),("@ip",ip)).Execute();
							}
						}
					}
				}
				var cleanr = new List<Guid>();
				foreach(var i in Redirect) if(i.Value.Timeout < DateTime.UtcNow) cleanr.Add(i.Key);
				foreach(var ri in cleanr) Redirect.TryRemove(ri, out _);
			}
		} else { /* TODO: throw something; */ }
	}

	static Auth() {
		HClient = new(){
			Timeout = new TimeSpan(0,0,Config.GetInt("Auth", "Timeout", 15)), 
			BaseAddress = new Uri($"https://{Config.GetVal("Auth", "Host")}/")
		};
		HClient.DefaultRequestHeaders.Add("X-Api-Key", Config.GetText("Auth", "Token"));
	}


	/// <summary>Vartotojo autorizacijos iniciavimas</summary>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	public static async Task<AuthRequest> GetAuth(HttpContext ctx, CancellationToken ct){
		LockIP(ctx);
		if(!ct.IsCancellationRequested) {
			var msg = new StringContent($"{{\"host\":\"{Config.GetVal("Auth","Redirect","http://localhost:5000/api/login")}\"}}", new MediaTypeHeaderValue("application/json"));			
			try {
				using var response = await HClient.PostAsync(Config.GetVal("Auth","GetSignin","/auth/evartai/sign"), msg, ct);
				var rsp = await response.Content.ReadAsStringAsync(ct);
				if(response.IsSuccessStatusCode){
					var tck = JsonSerializer.Deserialize<AuthTicket>(rsp);					
					if(tck?.Ticket is not null){
						var ath = new AuthRequest((Guid)tck.Ticket) { IP=ctx.GetIP(), Return = ctx.Request.Query.TryGetValue("r", out var r) ? r : "" };
						Redirect.TryAdd(ath.Ticket??new(),ath);
						ctx.Response.Redirect(tck.Url??"/");
						return ath;
					} else return new  AuthRequestError(1004,"Peradresavimo kodo klaida",rsp);
				} else return new  AuthRequestError(1003,"Peradresavimo klaida",rsp);
			} catch (Exception ex) { return new  AuthRequestError(1002,"Sujungimo klaida",ex.Message); }
		}
		return new AuthRequestError(0);
	}

	/// <summary>Vartotojo autorizacijos tikrinimas</summary>
	/// <param name="ticket">Autorizacijos kodas</param>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	public static async Task<AuthRequest> GetToken(Guid ticket, HttpContext ctx, CancellationToken ct){
		if(Redirect.TryRemove(ticket, out var tck)){
			if(tck.IP==ctx.GetIP()){
				if(tck.Timeout>DateTime.UtcNow){
					var m = new StringContent($"{{\"ticket\":\"{ticket}\",\"defaultGroupId\":null,\"refresh\":false}}",new MediaTypeHeaderValue("application/json"));			
					try {
						using var response = await HClient.PostAsync(Config.GetVal("Auth","GetLogin","/auth/evartai/login"), m, ct);
						var rsp = await response.Content.ReadAsStringAsync(ct);
						if(response.IsSuccessStatusCode){
							var tkn = JsonSerializer.Deserialize<AuthToken>(rsp);						
							if(!string.IsNullOrEmpty(tkn?.Token)){ 
								tck.Token=tkn.Token; return tck; 
							} else return new AuthRequestError(1010,"Negalimas prisijungimas",rsp);
						} else return new AuthRequestError(1009,"Autorizacijos klaida",rsp);
					} catch (Exception ex) { return new AuthRequestError(1008,"Prisijungimo validacijos klaida",ex.Message); }
				} else return new AuthRequestError(1007,"Baigėsi prisijungimui skirtas laikas",tck.Timeout.ToString("u"));
			} else return new AuthRequestError(1006,"Neteisingas prisijungimo adresas",tck.Timeout.ToString("u"));
		} else return new AuthRequestError(1005,"Neatpažinta autorizacija",ticket.ToString());
	}

	/// <summary>Sesijos sukūrimas</summary>
	/// <param name="req"></param>
	/// <param name="ctx"></param>
	/// <returns></returns>
	public static AuthRequest SessionInit(AuthRequest req, HttpContext ctx){
		var usr=req.User;
		if(usr is not null && long.TryParse(usr.AK, out var ak)){
			var usl = User.Login(ak,usr.FName??"",usr.LName??"",usr.Email,usr.Phone,usr.JA,ctx);
			if(usl is UserError err) return new AuthRequestError(err.Code,err.Message,JsonSerializer.Serialize(err.ErrorData));
			Session.CreateSession(usl,ctx);
			usr.AK=null;
			return req;
		}
		return new AuthRequestError(1014,"Vartotojo kodas neatpažintas",JsonSerializer.Serialize(usr));
	}

	/// <summary>Vartotojo autorizacijos detalės</summary>
	/// <param name="req">Autorizacijos užklausa</param>
	/// <param name="ct"></param>
	public static async Task<AuthRequest> GetUserDetails(AuthRequest req, CancellationToken ct){
		using var msg = new HttpRequestMessage(HttpMethod.Get, Config.GetVal("Auth","GetUser","/api/users/me?populate=groups"));
		msg.Headers.Authorization = new("Bearer",req.Token);
		try {
			using var response = await HClient.SendAsync(msg,ct);
			var rsp = await response.Content.ReadAsStringAsync(ct);
			if(response.IsSuccessStatusCode){
				var usr = JsonSerializer.Deserialize<AuthUser>(rsp);
				new DBExec("insert into app.log_login (log_data) VALUES (@dt);","@dt",rsp).Execute();
				if(!string.IsNullOrEmpty(usr?.AK)){ 
					req.User=usr; 
					return await GetUserGroups(req,ct);
				}
				else return new AuthRequestError(1013,"Vartotojas neatpažintas",rsp);
			} else return new AuthRequestError(1012,"Vartotojas nerastas",rsp);
		}
		catch (Exception ex) { return new AuthRequestError(1011,"Vartotojo informacijos klaida",ex.Message); }
	}

		/// <summary>Vartotojo autorizacijos detalės</summary>
	/// <param name="req">Autorizacijos užklausa</param>
	/// <param name="ct"></param>
	public static async Task<AuthRequest> GetUserGroups(AuthRequest req, CancellationToken ct){
		if(req.User is null) return req;
		var usr = req.User;
		using var msg = new HttpRequestMessage(HttpMethod.Get, Config.GetVal("Auth","GetUser",$"/api/users/{usr.ID}?populate=groups"));
		msg.Headers.Authorization = new("Bearer",req.Token);
		try {
			using var response = await HClient.SendAsync(msg,ct);
			var rsp = await response.Content.ReadAsStringAsync(ct);
			if(response.IsSuccessStatusCode){
				var inf = JsonSerializer.Deserialize<AuthUserInfo>(rsp);
				new DBExec("insert into app.log_login (log_data) VALUES (@dt);","@dt",rsp).Execute();
				if (inf?.Groups is not null) foreach(var i in inf.Groups) if(i.Email==usr.Email && i.Phone==usr.Phone) usr.JA=i.JA; 
				return req; 
			} else return new AuthRequestError(1016,"Vartotojas nerastas",rsp);
		}
		catch (Exception ex) { return new AuthRequestError(1015,"Vartotojo informacijos klaida",ex.Message); }
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
public class AuthRequest {
	/// <summary>Autorizacijos identifikavimo numeris</summary>
	public Guid? Ticket { get; set; }
	/// <summary>Vartotojo IP adresas</summary>
	public string? IP { get; set; }
	/// <summary>Vartotojo peradresavimas po autorizacijos</summary>
	public string? Return { get; set; }
	/// <summary>Vartotojo autorizacijos laiko limitas</summary>
	public DateTime Timeout { get; set; }
	/// <summary>Vartotojo autorizacijos raktas</summary>
	public string? Token { get; set; }
	/// <summary>Vartotojo duomenys</summary>
	public AuthUser? User { get; set; }
	
	/// <summary>Užklausos konstruktorius</summary>
	/// <param name="ticket">Autorizacijos identifikavimo numeris</param>
	public AuthRequest(Guid ticket){
		Ticket=ticket;
		Timeout=DateTime.UtcNow.AddSeconds(Config.GetLong("Auth", "Timeout", 300));
	}
	/// <summary>Užklausos bazinis konstruktorius</summary>
	public AuthRequest(){}

	/// <summary>Autorizacijos užklausos klaidų tikrinimas ir reportavimas</summary>
	/// <param name="ctx"></param>
	/// <param name="report"></param>
	/// <returns></returns>
	public bool Valid(HttpContext ctx, bool report=true){ if(this is AuthRequestError err) { if(report) err.Report(ctx); return false; } return true; }

}

/// <summary>Autorizacijos užklausos klaida</summary>
public class AuthRequestError : AuthRequest {

	/// <summary>Klaidos žinutės kodas</summary>
	public int Code { get; set; }
	/// <summary>Statuso žinutė</summary>
	public string? Message { get; set; }
	/// <summary>Klaidos informacija</summary>
	public string? ErrorData { get; set; }

	/// <summary>Bazinis konstruktorius</summary>
	public AuthRequestError(int code, string? msg="", string? data=null){ Code=code; Message=msg; ErrorData=data; }
	
	/// <summary>Reportuoti klaidą</summary>
	/// <param name="ctx"></param>
	/// <returns></returns>
	public AuthRequest Report(HttpContext ctx){
		new DBExec("INSERT INTO app.log_error (log_code,log_msg,log_data,log_ip) VALUES (@code,@msg,@data,@ip);",("@code",Code),("@msg",Message),("@data",ErrorData),("@ip",ctx.GetIP())).Execute();
		ctx.Response.Redirect(Return??$"/klaida?id={Code}{(Message is null?"":"&msg="+Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Message)))}");
		return this;
	}
}

/// <summary>BIIP Vartotojo detalės</summary>
public class AuthUser {
	/// <summary>BIIP ID</summary>
	[JsonPropertyName("id")] public int ID { get; set; }
	/// <summary>Juridinio asmens kodas</summary>
	[JsonPropertyName("companyCode")] public string? JA { get; set;}
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

/// <summary>JA Grupės</summary>
public class AuthGroups {
	/// <summary></summary>
	[JsonPropertyName("id")] public int ID { get; set; }
	/// <summary>BIIP identifikatorius</summary>
	[JsonPropertyName("companyCode")] public string? JA { get; set; }
	/// <summary>JA Kodas</summary>
	[JsonPropertyName("name")] public string? Name { get; set; }
	/// <summary>JA Pavadinimas</summary>
	[JsonPropertyName("companyEmail")] public string? Email { get; set; }
	/// <summary>Tel. Nr.</summary>
	[JsonPropertyName("companyPhone")] public string? Phone { get; set; }
}

/// <summary>Vartotojo informacija</summary>
public class AuthUserInfo {
	
	/// <summary>BIIP ID</summary>
	[JsonPropertyName("id")] public int ID { get; set; }
	/// <summary>Vardas</summary>
	[JsonPropertyName("firstName")] public string? FName { get; set; }
	/// <summary>Pavardė</summary>
	[JsonPropertyName("lastName")] public string? LName { get; set; }
	/// <summary>El. Paštas</summary>
	[JsonPropertyName("email")] public string? Email { get; set; }
	/// <summary>Tel. Nr.</summary>
	[JsonPropertyName("phone")] public string? Phone { get; set; }
	/// <summary>Pilnas vardas</summary>
	[JsonPropertyName("fullName")] public string? Name { get; set; }
	/// <summary>Deleguoti JA</summary>
	[JsonPropertyName("groups")] public List<AuthGroups>? Groups { get; set; }
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