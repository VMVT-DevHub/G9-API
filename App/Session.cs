using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace App;

/// <summary>Vartotojų sesijos</summary>
public static class Session {
	private static ConcurrentDictionary<string, UserSession> Cache { get; set; } = new();

	private static Random Rnd = new();
	private const string RndChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
	/// <summary>Atsitiktinio rakto funkcija</summary>
	/// <param name="length">Rakto ilgis</param>
	/// <returns>Raktas</returns>
	public static string RandomStr(int length=60) {
		var sb = new System.Text.StringBuilder();
		for (var i = 0; i < length; i++) { var c = RndChars[Rnd.Next(0, RndChars.Length)]; sb.Append(c); }
		return sb.ToString();
	}

	/// <summary>Gauti sesijos vartotoją</summary>
	/// <param name="ctx"></param>
	/// <returns>Vartotojo informacija</returns>
	public static User? GetUser(this HttpContext ctx){
		if(ctx.Items.TryGetValue("User", out var usr) && usr is not null) { return (User)usr; }
		return null;
	}

	/// <summary>Gauti realų vartotojo IP adresą</summary>
	/// <param name="ctx"></param>
	/// <returns></returns>
	public static string GetIP(this HttpContext ctx) {
		//X-Forwarded-For
		if(ctx.Request.Headers.TryGetValue("X-Forwarded-For",out var ipv)){
			var ips = ipv.FirstOrDefault();
			if(!string.IsNullOrEmpty(ips)){
				var ip = ips.Split(", ").LastOrDefault();
				if(!string.IsNullOrEmpty(ip)) return ip;
			}
		}
		return ctx.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
	}

	/// <summary>Gauti vartotojo naršyklės informaciją</summary>
	/// <returns>User Agent</returns>
	public static string GetUA(this HttpContext ctx) => ctx.Request.Headers.UserAgent.FirstOrDefault() ?? "";

	/// <summary>Vartotojo autorizacijos validavimas</summary>
	/// <param name="ctx"></param>
	/// <returns>T/F jeigu vartotojas prisijungęs</returns>
	public static bool GetAuth(this HttpContext ctx){
		if(!ctx.Request.Cookies.TryGetValue("SSID",out var ssid) || string.IsNullOrEmpty(ssid)) return false;
		if(!Cache.TryGetValue(ssid, out var sess)) {
			using var db = new DBExec("SELECT sess_data,sess_expire,sess_extended FROM app.session WHERE sess_key=@ssid","@ssid",ssid);
			using var rdr = db.GetReader();
			if(!rdr.Read()) return false;
			sess = new UserSession(){ SSID=ssid, User=rdr.GetObject<User>(0), Expire=rdr.GetDateTime(1), Extended=rdr.GetInt16(2) };
		}
		if(sess.Expire < DateTime.UtcNow) { DropSession(ssid, ctx); return false; }
		if(sess.Extend < DateTime.UtcNow) ExtendSession(sess, ctx);
		ctx.Items["User"] = sess.User; return true;

		//TODO: Clean sessions
	}

	/// <summary>Sukurti naują vartotojo sesiją</summary>
	/// <param name="usr">Vartotojo detalės</param>
	/// <param name="ctx"></param>
	public static void CreateSession(User usr, HttpContext ctx){
		var sess = new UserSession(usr);
		new DBExec("INSERT INTO app.session (sess_key,sess_data,sess_expire,sess_extended) VALUES (@ssid,@data,@exp,0);",
			("@ssid",sess.SSID),("@data",JsonSerializer.Serialize(sess.User)),("@exp",sess.Expire)).Execute();
		AddCookie(sess, ctx);
	}

	/// <summary>Pratęsti vartotojo sesiją</summary>
	/// <param name="sess">Esama sesija</param>
	/// <param name="ctx"></param>
	public static void ExtendSession(UserSession sess, HttpContext ctx){
		sess.Extend=DateTime.UtcNow.AddSeconds(1); //pratęsti atnaujinimą
		var newsid = new UserSession(sess); //sukurti naują sesiją		
		new DBExec("UPDATE app.session SET sess_key=@newsid,sess_expire=@exp,sess_extended=@ext WHERE sess_key=@ssid;",
			("@ssid",sess.SSID),("@newsid",newsid.SSID),("@exp",newsid.Expire),("@ext",newsid.Extended)).Execute();
		AddCookie(newsid, ctx);
		Cache.TryRemove(sess.SSID, out _);
	}

	private static void AddCookie(UserSession sess, HttpContext ctx){
		Cache[sess.SSID] = sess;
		ctx.Response.Cookies.Append("SSID", sess.SSID, new CookieOptions(){ 
			//TODO: Set secure cookie for non-dev
			SameSite=SameSiteMode.Lax, 
			HttpOnly=true, Secure=true, Path="/api/", IsEssential=true, Expires = sess.Expire
		});
	}


	/// <summary>Pašalinti vartotojo sesiją</summary>
	/// <param name="ssid">Sesijos raktas</param>
	/// <param name="ctx"></param>
	public static void DropSession(string ssid, HttpContext? ctx=null){
		Cache.TryRemove(ssid, out _);
		new DBExec("UPDATE app.session SET sess_expire=(now() at time zone 'utc') WHERE sess_key=@ssid;","@ssid",ssid).Execute();
		ctx?.Response.Cookies.Delete("SSID");
	}
}

/// <summary>Vartotojo sesijos modelis</summary>
public class UserSession {
	/// <summary>Sesijos raktas</summary>
	public string SSID { get; set; } = Session.RandomStr(Config.GetInt("Session", "KeyLength", 64));
	/// <summary>Sesijos galiojimo laikas</summary>
	public DateTime Expire { get; set; } = DateTime.UtcNow.AddSeconds(Config.GetLong("Session", "Expire", 60));
	/// <summary>Sesijos pratęsimo laikas</summary>
	public DateTime Extend { get; set; } = DateTime.UtcNow.AddSeconds(Config.GetLong("Session", "Extend", 10));
	/// <summary>Extended session count</summary>
	public int Extended { get; set; }
	/// <summary>Vartotojo detalės</summary>
	public User? User { get; set; }

	/// <summary>Vartotojo sesijos konstruktorius</summary>
	public UserSession(){}
	/// <summary>Vartotojo sesijos konstruktorius</summary>
	/// <param name="usr">Vartotojo informacija</param>
	public UserSession(User usr){ User = usr; }
	/// <summary>Atnaujinti sesiją</summary>
	/// <param name="ssid">Sena sesija</param>
	public UserSession(UserSession ssid) {
		User = ssid.User; Extended=ssid.Extended+1;
	}
}

/// <summary>Vartotojo informacija</summary>
public class User {
	/// <summary>Sisteminis vartotojo id</summary>
	public Guid? ID { get; set; }
	/// <summary>Asmens Kodas</summary>
	public long? AK { get; set; }
	/// <summary>Vardas</summary>
	public string? FName { get; set; }
	/// <summary>Pavardė</summary>
	public string? LName { get; set; }
	/// <summary>El.Paštas</summary>
	public string? Email { get; set; }
	/// <summary>Telefono numeris</summary>
	public string? Phone { get; set; }
	/// <summary>Prisijungusio vartotojo tipas</summary>
	public string? Type { get; set; } = "USER";

	/// <summary>Vartotojo klaidų tikrinimas ir reportavimas</summary>
	/// <param name="ctx"></param>
	/// <returns></returns>
	public bool Valid(HttpContext ctx){ if(this is UserError err) { err.Report(ctx); return false; } return true; }

	/// <summary>Registruoti prisijungusį vartotoją</summary>
	/// <param name="ak">Asmens Kodas</param>
	/// <param name="fname">Vardas</param>
	/// <param name="lname">Pavardė</param>
	/// <param name="email">El.Paštas</param>
	/// <param name="phone">Telefonas</param>
	/// <param name="ctx"></param>
	/// <returns>Prisijungęs vartotojas</returns>
	public static User Login(long ak, string fname, string lname, string? email, string? phone, HttpContext ctx){
		var usr = new User(){ AK=ak, FName=fname, LName=lname, Email=email, Phone=phone };
		var lgn = Login(usr,ctx);
		if(lgn.Valid(ctx)) {

		}
		return lgn;
	}

	private static User Login(User usr, HttpContext ctx, bool retry=false){
		using var db = new DBExec("SELECT id,ak,fname,lname,email,phone FROM app.user_login(@ak)",("@ak",usr.AK),("@ip",ctx.GetIP()),("@ua",ctx.GetUA()));
		using var rdr = db.GetReader();
		if(rdr.Read()){
			if(!(rdr.IsDBNull(0) || rdr.IsDBNull(1) || rdr.IsDBNull(2) || rdr.IsDBNull(3))) {
				var usd = new User(){ ID=rdr.GetGuid(0), AK=rdr.GetInt64(1), FName=rdr.GetString(2), LName=rdr.GetString(3), Email=rdr.GetStringN(4), Phone=rdr.GetStringN(5) };
				if(usr.FName!=usd.FName || usr.LName!=usd.LName || usr.Email!=usd.Email || usr.Phone!=usd.Phone){
					Update(usd);
					//check error
				} 
				return usd;
			}
			return new UserError(1101, "Vartotojo informacijos klaida", usr);
		} else return Create(usr);
	}
	private static User Create(User usr){
		using var db = new DBExec().Execute();
		return usr;
	}
	private static User Update(User usr){
		using var db = new DBExec().Execute();
		return usr;
	}
}

/// <summary>Vartotojo prisijungimo klaidos išplėtimas</summary>
public class UserError : User {
	/// <summary>Klaidos kodas</summary>
	public int Code { get; set; }
	/// <summary>Klaidos žnutė</summary>
	public string Message { get; set; }
	/// <summary>Klaidos duomenys</summary>
	public string? ErrorData { get; set; }

	/// <summary>Vartotojo prisijungimo klaidos konstruktorius</summary>
	/// <param name="code"></param>
	/// <param name="msg"></param>
	/// <param name="data"></param>
	public UserError(int code, string msg, string? data){ Code=code; Message=msg; ErrorData=data; }
	
	/// <summary>Vartotojo prisijungimo klaidos konstruktorius</summary>
	/// <param name="code"></param>
	/// <param name="msg"></param>
	/// <param name="data"></param>
	public UserError(int code, string msg, object? data){ Code=code; Message=msg; ErrorData=JsonSerializer.Serialize(data); }

	/// <summary>Reportuoti klaidą</summary>
	/// <param name="ctx"></param>
	/// <returns></returns>
	public UserError Report(HttpContext ctx){
		new DBExec("INSERT INTO app.log_error (log_code,log_msg,log_data,log_ip) VALUES (@code,@msg,@data,@ip);",
			("@code",Code),("@msg",Message),("@data",ErrorData),("@ip",ctx.GetIP())).Execute();
		ctx.Response.Redirect($"/klaida?id={Code}&msg={Convert.ToBase64String(Encoding.UTF8.GetBytes(Message))}");
		return this;
	}
}


class RequireLogin : IEndpointFilter {
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next) 
		=> ctx.HttpContext.GetAuth() ? await next(ctx) : E401.Response;
}


public static class Require {
	public static async ValueTask<object?> Login(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next) 
		=> ctx.HttpContext.GetAuth() ? await next(ctx) : E401.Response;
	public static async ValueTask<object?> Admin (EndpointFilterInvocationContext ctx, EndpointFilterDelegate next){
		if (ctx.HttpContext.GetAuth()){
			return await next(ctx);
		}
		return E401.Response;
	}
}


/// <summary>Vartotojo autorizacijos klaida</summary>
public class E401 {
	/// <summary>Statinis klaidos atsakymas</summary>
	public static readonly E401 Response = new();
	/// <summary>Klaidos kodas</summary>
	/// <example>401</example>
	public int Code { get; set; } = 401;
	/// <summary>Klaidos statusas</summary>
	/// <example>Unauthorized</example>
	public string Status { get; set; } = "Unauthorized";
	/// <summary>Klaidos Žinutė</summary>
	/// <example>Authorization is required for this resource</example>
	public string Message { get; set; } = "Authorization is required for this resource";
}


/// <summary>Vartotojo prieigos klaida</summary>
public class E403 {
	/// <summary>Statinis klaidos atsakymas</summary>
	public static readonly E403 Response = new();
	/// <summary>Klaidos kodas</summary>
	/// <example>403</example>
	public int Code { get; set; } = 403;
	/// <summary>Klaidos statusas</summary>
	/// <example>Forbidden</example>
	public string Status { get; set; } = "Forbidden";
	/// <summary>Klaidos Žinutė</summary>
	/// <example>Authorization is required for this resource</example>
	public string Message { get; set; } = "You don't have permission to access this resource";
}