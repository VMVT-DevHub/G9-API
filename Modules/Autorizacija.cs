namespace G9.API;

/// <summary>Vartotojo autorizacija</summary>
public static class Auth {
	/// <summary>Vartotojo informacija</summary>
	/// <param name="ctx">Http Context</param>
	/// <param name="ct">Cancellation Token</param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx,CancellationToken ct){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync("[]",ct);
	}

	/// <summary>Vartotojo prisijungimas</summary>
	/// <param name="ctx">Http Context</param>
	/// <param name="ct">Cancellation Token</param>
	public static async Task Login(HttpContext ctx,CancellationToken ct){
		await ctx.Response.WriteAsync("[]",ct);
	}
}