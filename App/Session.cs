using System.Collections.Concurrent;
using System.Data;
using System.Net.Http.Headers;
using System.Text.Json;
using App.Users;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace App;

/// <summary>Vartotojų sesijos</summary>
public static class Session {
	private static ConcurrentDictionary<string, UserSession> Cache { get; set; } = new();
	private static ConcurrentDictionary<string, ApiKey> ApiCache { get; set; } = new();
	private static ConcurrentDictionary<string, ApiRequest> ApiRequests { get; set; } = new();
	

	private static readonly Random Rnd = new();
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
			Cache[ssid] = sess;
		}
		if(sess.Expire < DateTime.UtcNow) { DropSession(ssid, ctx); return false; }
		if(sess.Extend < DateTime.UtcNow) ExtendSession(sess, ctx);
		ctx.Items["User"] = sess.User; return true;

		//TODO: Clean sessions
	}


	private static int ApiAuthClean { get; set; }
	private static int ApiAuthCleanMax { get; set; }
	/// <summary>API autorizacijos validavimas</summary>
	/// <param name="ctx"></param>
	/// <returns>T/F jei API raktas egzidtuoja</returns>
	public static bool GetApiAuth(this HttpContext ctx){
		var now = DateTime.UtcNow;
		ApiAuthClean++;
		if(ApiAuthClean>ApiAuthCleanMax) {
			ApiAuthCleanMax=Config.GetInt("Session","ApiAuthClean",100);
			ApiAuthClean = 0; var cln = new List<string>(); 
			foreach(var i in ApiCache) if(i.Value.Refresh<now) cln.Add(i.Key);
			foreach (var i in cln) ApiCache.TryRemove(i, out _);
		}
		if(ctx.Request.Headers.TryGetValue("X-API-Key", out var k)){
			string key = k.FirstOrDefault()??"";
			if(key.Length>0){
				if(ApiCache.TryGetValue(key, out var apk) && apk.Refresh>now){
					if(apk.Key is null) ApiCheck(ctx,key);
					else { ctx.Items["ApiKey"]=apk; return true; }
				} else {
					ApiCheck(ctx,key);
					var rfr = now.AddSeconds(Config.GetInt("Session","ApiCache",60));
					using var db = new DBExec("SELECT \"ID\",\"Deklar\",\"Exp\",\"Metai\" FROM app.api_auth(@key);", "@key", key);
					using var rdr = db.GetReader();
					if(rdr.Read()){
						ctx.Items["ApiKey"] = ApiCache[key] = new(){ ID=rdr.GetGuid(0), Key=key, Refresh=rfr, Deklaracija=rdr.GetIntN(1)??0, Expire=DateOnly.FromDateTime(rdr.GetDateTimeN(2)??now) ,Metai=rdr.GetIntN(3)??2000 };
						return true;
					}
					ApiCache[key]= new(){ Key=null, Refresh=rfr };
				}
			}
		}
		//TODO: Clean sessions
		return false;
	}

	private static int ApiCacheClean { get; set; }
	private static int ApiCacheCleanMax { get; set; }
	private static void ApiCheck(HttpContext ctx, string key){
		var ip = ctx.GetIP(); var now=DateTime.UtcNow;
		var rls = now.AddSeconds(Config.GetInt("Session","ApiRelease",30));
		if(ApiRequests.TryGetValue(ip, out var chk) && chk.Release>now){
			lock(chk){
				Thread.Sleep(chk.Count*Config.GetInt("Session","ApiSleep",500)*(chk.LastKey!=key?2:1));
				chk.Count++; chk.Release=rls;
			}
		} 
		else { ApiRequests[ip] = new(){ Count=1, IP=ip, LastKey=key, Release=rls }; }
		ApiCacheClean++;
		if(ApiCacheClean>ApiCacheCleanMax){
			ApiCacheCleanMax=Config.GetInt("Session","ApiCacheClean",100);
			ApiCacheClean = 0; var cln = new List<string>(); 
			foreach(var i in ApiRequests) if(i.Value.Release<now) cln.Add(i.Key);
			foreach (var i in cln) ApiRequests.TryRemove(i, out _);
		}
	}

	/// <summary>Gauti API autorizaciją</summary>
	/// <param name="ctx"></param>
	/// <returns>API informacija</returns>
	public static ApiKey? GetAPI(this HttpContext ctx) => ctx.Items.TryGetValue("ApiKey", out var api) && api is not null ? (ApiKey)api : null;

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
			HttpOnly=true, Secure=true, Path=$"{Config.GetVal("Web","Path","/")}", IsEssential=true, Expires = sess.Expire
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
	public UserSession(User usr){ User = usr;  User.SessionExpire=Expire; User.SessionExtend=Extend; }
	/// <summary>Atnaujinti sesiją</summary>
	/// <param name="ssid">Sena sesija</param>
	public UserSession(UserSession ssid) {
		if (ssid.User is not null){ User = ssid.User; User.SessionExpire=Expire; User.SessionExtend=Extend; }
		Extended=ssid.Extended+1;
	}
}

/// <summary>API prieigos modelis</summary>
public class ApiKey {
	/// <summary>API prieigos ID</summary>
	public Guid? ID { get; set; }
	/// <summary>Autorizacijos raktas</summary>
	public string? Key { get; set; }
	/// <summary>Deklaracijos ID</summary>
	public int Deklaracija { get; set; }
	/// <summary>Rakto galiojimo laikas</summary>
	public DateOnly Expire { get; set; }
	/// <summary>Deklaracijos metai</summary>
	public int Metai { get; set; }
	
	/// <summary>Rakto "cache' atnaujinimo laikas</summary>
	public DateTime Refresh { get; set; }
}

/// <summary>API prieigos užklausų apsauga</summary>
public class ApiRequest {
	/// <summary>IP adresas</summary>
	public string? IP { get; set; }
	/// <summary>Užklausų skaičius</summary>
	public int Count { get; set; }
	/// <summary>Paskurinis prisijungimo raktas</summary>
	public string? LastKey { get; set; }
	/// <summary>BLokavimo paleidimo laikas</summary>
	public DateTime Release { get; set; }
}