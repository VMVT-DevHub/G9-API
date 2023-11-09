using System.Collections.Concurrent;
using System.Text.Json;
using App.Users;

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
