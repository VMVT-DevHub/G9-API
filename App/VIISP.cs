using App.Users;
using System.Collections.Concurrent;
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
			lock(lck){ lck.LastLock=DateTime.UtcNow; lck.Count++; var dly = Config.GetInt("Auth", "LockDelay", 1); if(dly>0) Thread.Sleep(dly); }
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
			BaseAddress = new Uri($"{Config.GetVal("Auth", "Host")}/")			
		};
		var tkn = Config.GetText("Auth", "Token");
		if(!string.IsNullOrEmpty(tkn)) HClient.DefaultRequestHeaders.Add("X-Api-Key", tkn);
	}


	/// <summary>Vartotojo autorizacijos iniciavimas</summary>
	/// <param name="ctx"></param>
	/// <param name="r"></param>
	/// <param name="ct"></param>
	public static async Task<AuthRequest> GetAuth(HttpContext ctx, string? r, CancellationToken ct){
		LockIP(ctx);
		if(!ct.IsCancellationRequested) {			
			try {
				using var response = await HClient.GetAsync("", ct);
				var rsp = await response.Content.ReadAsStringAsync(ct);
				if(response.IsSuccessStatusCode){
					var tck = JsonSerializer.Deserialize<AuthTicket>(rsp);					
					if(tck?.Ticket is not null){
						var ath = new AuthRequest((Guid)tck.Ticket) { IP=ctx.GetIP(), Return = r ?? Config.GetVal("Auth", "Return") };
						Redirect.TryAdd(ath.Ticket??new(),ath);
						ctx.Response.Redirect(tck.Url??"/");
						return ath;
					} else return new  AuthRequestError(1004,"Peradresavimo kodo klaida",rsp);
				} else return new  AuthRequestError(1003,"Peradresavimo klaida",rsp);
			} catch (Exception ex) { return new  AuthRequestError(1002,"Sujungimo klaida",ex.Message); }
		}
		return new AuthRequestError(0);
	}

	/// <summary>Sesijos sukūrimas</summary>
	/// <param name="req"></param>
	/// <param name="ctx"></param>
	/// <returns></returns>
	public static AuthRequest SessionInit(AuthRequest req, HttpContext ctx){
		var usr=req.User;
		if(usr?.Id is not null){
			var usl = User.Login(usr.Id??new(),FixCase(usr.FName),FixCase(usr.LName),usr.Email,usr.Phone,usr.CompanyCode,ctx);
			if(usl is UserError err) return new AuthRequestError(err.Code,err.Message,JsonSerializer.Serialize(err.ErrorData));
			Session.CreateSession(usl,ctx);
			return req;
		}
		return new AuthRequestError(1014,"Vartotojo kodas neatpažintas",JsonSerializer.Serialize(usr));
	}
	private static string FixCase(string? txt) => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(txt?.ToLower()??"");

	/// <summary>Vartotojo autorizacijos tikrinimas</summary>
	/// <param name="token">Autorizacijos kodas</param>
	/// <param name="ticket">Autorizacijos kodas</param>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	public static async Task<AuthRequest> GetUser(Guid token, Guid ticket, HttpContext ctx, CancellationToken ct){
		if(Redirect.TryRemove(token, out var tck) && token!=ticket){
			if(tck.IP==ctx.GetIP()){
				if(tck.Timeout>DateTime.UtcNow){
					try {
						using var response = await HClient.GetAsync($"{token}", ct);
						var rsp = await response.Content.ReadAsStringAsync(ct);
						if(response.IsSuccessStatusCode){
							var usr = JsonSerializer.Deserialize<AuthUser>(rsp);
							if(usr?.Id is not null){
								tck.User=usr; return tck; 
							} else return new AuthRequestError(1010,"Negalimas prisijungimas",rsp);
						} else return new AuthRequestError(1009,"Autorizacijos klaida",rsp);
					} catch (Exception ex) { return new AuthRequestError(1008,"Prisijungimo validacijos klaida",ex.Message); }
				} else return new AuthRequestError(1007,"Baigėsi prisijungimui skirtas laikas",tck.Timeout.ToString("u"));
			} else return new AuthRequestError(1006,"Neteisingas prisijungimo adresas",tck.Timeout.ToString("u"));
		} else return new AuthRequestError(1005,"Neatpažinta autorizacija",token.ToString());
	}

	/// <summary>Gauti vartotojo duomenis pagal ID ar AK</summary>
	/// <param name="id">Vartotojo ID</param>
	/// <param name="ct"></param>
	/// <returns>Vartotojo informacija</returns>
	public static async Task<AuthUser?> GetUser(object id, CancellationToken ct) {
		using var response = await HClient.GetAsync($"user/{id}", ct);
		var rsp = await response.Content.ReadAsStringAsync(ct);
		return response.IsSuccessStatusCode ? JsonSerializer.Deserialize<AuthUser>(rsp) : null;
	}
	
	/// <summary>Gauti vartotojo duomenis pagal ID ar AK</summary>
	/// <param name="usr">Vartotojo duomenys</param>
	/// <param name="ct"></param>
	/// <returns>Vartotojo informacija</returns>
	public static async Task<AuthUser?> SetUser(AuthUser usr, CancellationToken ct) {
		using var response = await HClient.PostAsJsonAsync($"user", usr, ct);
		var rsp = await response.Content.ReadAsStringAsync(ct);
		return response.IsSuccessStatusCode ? JsonSerializer.Deserialize<AuthUser>(rsp) : null;
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
		ctx.Response.Redirect(Return??$"{Config.GetVal("Web","Path","/")}klaida?id={Code}{(Message is null?"":"&msg="+Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Message)))}");
		return this;
	}
}

/// <summary>Vartotojo informacija</summary>
public class AuthUser {
	/// <summary>Vartotojo ID</summary>
	[JsonPropertyName("id")] public Guid? Id { get; set; }
	/// <summary>Vartotojo AK</summary>
	[JsonPropertyName("ak")] public long? AK { get; set; }
	/// <summary>Pilnas vardas</summary>
	[JsonPropertyName("name")] public string? Name { get; set; }
	/// <summary>Vardas</summary>
	[JsonPropertyName("firstName")] public string? FName { get; set; }
	/// <summary>Pavardė</summary>
	[JsonPropertyName("lastName")] public string? LName { get; set; }
	/// <summary>El. Paštas</summary>
	[JsonPropertyName("email")] public string? Email { get; set; }
	/// <summary>Tel. Nr.</summary>
	[JsonPropertyName("phone")] public string? Phone { get; set; }
	/// <summary>Juridinio asmens kodas</summary>
	[JsonPropertyName("companyCode")] public string? CompanyCode { get; set; }
	/// <summary>Juridinio asmens pavadinimas</summary>
	[JsonPropertyName("companyName")] public string? CompanyName { get; set; }

}


/// <summary>VIISP Autorizacija</summary>
public class AuthTicket {
	/// <summary>Autorizacijos kodas</summary>
	[JsonPropertyName("ticket")] public Guid? Ticket { get; set; }
	/// <summary>VIISP peradresavimas</summary>
	[JsonPropertyName("authUrl")] public string? Url { get; set; }
}

/// <summary>VIISP Autorizacijos atsakas</summary>
public class AuthReturn {
	/// <summary>Autorizacijos kodas</summary>
	[JsonPropertyName("ticket")] public string? Ticket { get; set; }
	/// <summary>VIISP adresas</summary>
	[JsonPropertyName("customData")] public string? customData { get; set; }
}
