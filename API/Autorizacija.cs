using App.VIISP;
using App.Users;
using App.Auth;
using System.Security.Principal;

namespace App.API;

/// <summary>Vartotojo autorizacija</summary>
public static class Auth {


	/// <summary>Vartotojo informacija</summary>
	/// <param name="ctx">Http Context</param>
	/// <returns></returns>
	public static User? Get(HttpContext ctx){
		ctx.Response.ContentType="application/json";
		return ctx.GetUser();
	}

	/// <summary>Vartotojo prisijungimas</summary>
	/// <param name="ctx">Http Context</param>
	/// <param name="ct">Cancellation Token</param>
	/// <param name="r">Peradresavimo kelias</param>
	public static async Task Login(HttpContext ctx, CancellationToken ct, string? r=null){
		if(await VIISP.Auth.GetAuth(ctx,r,ct) is AuthRequestError err) err.Report(ctx);
	}
	
	/// <summary>Vartotojo atsijungimas</summary>
	/// <param name="ctx">Http Context</param>
	/// <param name="r">Peradresavimo kelias</param>
	public static void Logout(HttpContext ctx, string? r=null){
		if(ctx.Request.Cookies.TryGetValue("SSID",out var ssid) && !string.IsNullOrEmpty(ssid)) Session.DropSession(ssid,ctx);
		ctx.Response.Redirect(TestUrl(r));
	}


	private static string TestUrl(string? url) {
		try {
			if (!string.IsNullOrEmpty(url)) {
				var uri = new Uri(url);
				if (uri.IsLoopback || uri.Host.EndsWith("vmvt.lt", StringComparison.OrdinalIgnoreCase)) {
					return url;
				}
			}
		} catch (Exception) { }
		return "/";
	}

	/// <summary>Vartotojo prisijungimas</summary>
	/// <param name="ctx">Http Context</param>
	/// <param name="ct">Cancellation Token</param>
	public static async Task Evartai(HttpContext ctx, CancellationToken ct){
		if(ctx.Request.HasFormContentType && ctx.Request.Form.TryGetValue("ticket", out var tks)){
			if(Guid.TryParse(tks, out var ticket)){
				var tkn = await VIISP.Auth.GetUser(ticket,ctx,ct);
				if(tkn.Valid(ctx) && tkn?.User is not null) {
					tkn = VIISP.Auth.SessionInit(tkn, ctx);				
					if(tkn.Valid(ctx)) {
						var ret = tkn.Return ?? "/";
						ctx.Response.Redirect(string.IsNullOrEmpty(ret) ? "/" : ret);					
					}
				}
			}
		}
	}

#if DEBUG
	/// <summary>Prisijungti kaip juridinis asmuo</summary>
	/// <param name="ctx">Http Context</param>
	/// <param name="ja">Juridinio asmens kodas</param>
	/// <param name="ct">Cancellation Token</param>
	public static async Task Impersonate(HttpContext ctx, long ja, CancellationToken ct){
		//TODO: laikinas taisymas, pašalinti
		if (ctx.Request.Host.Host.StartsWith("test.g9")) {
			ctx.Response.Redirect($"https://{ctx.Request.Host.Value.Replace("test.g9","g9.test")}{ctx.Request.Path}", permanent: true);
		}
		else {
			if (ja == 1234) {
				VIISP.Auth.SessionInit(new() { User = new() { Id = new Guid(), Email = "test@vmvt.lt", Phone = "+37060000000", FName = "Test", LName = "Test" } }, ctx);
			}
			if (ctx.GetAuth()) {
				using var db = new DBExec("SELECT ja_pavadinimas,adresas FROM jar.data WHERE ja_kodas=@id;", "@id", ja);
				using var rdr = await db.GetReader(ct);
				var usr = ctx.GetUser();
				if (usr is not null && rdr.HasRows && await rdr.ReadAsync(ct)) {
					//TODO: Add log
					usr.JA = new() { ID = ja, Title = rdr.GetStringN(0), Addr = rdr.GetStringN(1) };
					usr.GetRoles();
				}
				else Error.E404(ctx, true);
			}
			else Error.E401(ctx, true);
		}
	}
#endif
}