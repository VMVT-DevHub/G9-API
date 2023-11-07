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
	public static async Task Login(HttpContext ctx,CancellationToken ct){
		var req = await VIISP.Auth.GetAuth(ctx,ct);
		if(req.Code>0) req.Report(ctx);
	}
	
	/// <summary>Vartotojo prisijungimas</summary>
	/// <param name="ctx">Http Context</param>
	/// <param name="ticket">Autorizacijos numeris</param>
	/// <param name="customData">Popildomos prisijungimo detalės</param>
	/// <param name="ct">Cancellation Token</param>
	public static async Task Evartai(HttpContext ctx, Guid ticket, string customData, CancellationToken ct){
		var tkn = await VIISP.Auth.GetToken(ticket,ctx,ct);
		if(tkn.Code>0) tkn.Report(ctx);
		else if(tkn?.Token is not null) {
			tkn = await VIISP.Auth.GetUserDetails(tkn, ct);
			if(tkn.Code>0) tkn.Report(ctx);
			else if(tkn?.User is not null) {
				VIISP.Auth.SessionInit(tkn.User, ctx);
				var ret = tkn.Return ?? "/";
				ctx.Response.Redirect(string.IsNullOrEmpty(ret) ? "/" : ret);
			}
		}
	}
}