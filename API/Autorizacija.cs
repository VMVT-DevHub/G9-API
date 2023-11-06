using System.Text.Json;

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
		
		var m = await App.VIISP.Auth.GetAuth(ctx,ct);
		await ctx.Response.WriteAsync("[]",ct);
	}
	
	/// <summary>Vartotojo prisijungimas</summary>
	/// <param name="ctx">Http Context</param>
	/// <param name="ticket">Autorizacijos numeris</param>
	/// <param name="customData">Popildomos prisijungimo detalės</param>
	/// <param name="ct">Cancellation Token</param>
	public static async Task Evartai(HttpContext ctx, Guid ticket, string customData, CancellationToken ct){
		var m = await App.VIISP.Auth.GetUser(ticket,ct);
		await ctx.Response.WriteAsync(JsonSerializer.Serialize(m),ct);
	}
}