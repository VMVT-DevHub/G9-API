using App.VIISP;
using App.Users;
using App.Auth;

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
		ctx.Response.Redirect(string.IsNullOrEmpty(r) ? "/" : r);
	}

	/// <summary>Vartotojo prisijungimas</summary>
	/// <param name="ctx">Http Context</param>
	/// <param name="ticket">Autorizacijos numeris</param>
	/// <param name="customData">Popildomos prisijungimo detalės</param>
	/// <param name="ct">Cancellation Token</param>
	public static async Task Evartai(HttpContext ctx, Guid ticket, string customData, CancellationToken ct){
		var tkn = await VIISP.Auth.GetToken(ticket,ctx,ct);
		if(tkn.Valid(ctx) && tkn?.Token is not null) {
			tkn = await VIISP.Auth.GetUserDetails(tkn, ct);
			if(tkn.Valid(ctx) && tkn?.User is not null) {
				tkn = VIISP.Auth.SessionInit(tkn, ctx);				
				if(tkn.Valid(ctx)) {
					var ret = tkn.Return ?? "/";
					ctx.Response.Redirect(string.IsNullOrEmpty(ret) ? "/" : ret);
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
		using var db = new DBExec("SELECT ja_title,ja_adresas FROM jar.data WHERE ja_id=@id;","@id",ja);
		using var rdr = await db.GetReader(ct);
		var usr = ctx.GetUser();
		if(usr is not null && rdr.HasRows && await rdr.ReadAsync(ct)){
			//TODO: Add log
			usr.JA = new () { ID=ja, Title=rdr.GetStringN(0), Addr=rdr.GetStringN(1) };
			usr.GetRoles();
		}
		else Error.E404(ctx,true);
	}
#endif
}