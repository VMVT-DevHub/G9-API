using Npgsql.Replication;

namespace App.Auth;


/// <summary>Autorizacijos reikalavimo funkcijos</summary>
public static class Require {
	/// <summary>Privaloma autorizacija</summary>
	/// <param name="ctx"></param><param name="next"></param><returns></returns>
	public static async ValueTask<object?> Login(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next) 
		=> ctx.HttpContext.GetAuth() ? await next(ctx) : Error.E401(ctx.HttpContext);

	/// <summary>Vartotojas turi turėti bent vieną rolę</summary>
	/// <param name="ctx"></param><param name="next"></param><returns></returns>
	public static async ValueTask<object?> Role (EndpointFilterInvocationContext ctx, EndpointFilterDelegate next) =>
		ctx.HttpContext.GetAuth() ? (ctx.HttpContext.GetUser()?.Roles?.Count>0 ? await next(ctx) : Error.E403(ctx.HttpContext)) : Error.E401(ctx.HttpContext);
	
	/// <summary>Vartotojas turi turėti bent vieną administratoriaus rolę</summary>
	/// <param name="ctx"></param><param name="next"></param><returns></returns>
	public static async ValueTask<object?> AdminRole (EndpointFilterInvocationContext ctx, EndpointFilterDelegate next) =>
		ctx.HttpContext.GetAuth() ? (ctx.HttpContext.GetUser()?.Admin?.Count>0 ? await next(ctx) : Error.E403(ctx.HttpContext)) : Error.E401(ctx.HttpContext);
}


/// <summary>Klaidos standartinis modelis</summary>
public class Error {
	/// <summary>Klaidos kodas</summary>
	public virtual int Code { get; set; }

	private static T Respond<T>(T obj, HttpContext ctx, bool print=false) where T : Error {
		ctx.Response.StatusCode=obj.Code;
		if(print) ctx.Response.WriteAsJsonAsync(obj);
		return obj;
	}

	/// <summary>Autorizacijos klaida</summary>
	public static E401 E401(HttpContext ctx, bool print=false) => Respond(Er401,ctx,print);
	private static E401 Er401 {get;} = new();
	/// <summary>Prieigos klaida</summary>
	public static E403 E403(HttpContext ctx, bool print=false) => Respond(Er403,ctx,print);
	private static E403 Er403 { get; } = new();
}




/// <summary>Vartotojo autorizacijos klaida</summary>
public class E401 : Error {
	/// <summary>Klaidos kodas</summary>
	/// <example>401</example>
	public override int Code { get; set; } = 401;
	/// <summary>Klaidos statusas</summary>
	/// <example>Unauthorized</example>
	public string Status { get; set; } = "Unauthorized";
	/// <summary>Klaidos Žinutė</summary>
	/// <example>Authorization is required for this resource</example>
	public string Message { get; set; } = "Authorization is required for this resource";
}


/// <summary>Vartotojo prieigos klaida</summary>
public class E403 : Error {
	/// <summary>Klaidos kodas</summary>
	/// <example>403</example>
	public override int Code { get; set; } = 403;
	/// <summary>Klaidos statusas</summary>
	/// <example>Forbidden</example>
	public string Status { get; set; } = "Forbidden";
	/// <summary>Klaidos Žinutė</summary>
	/// <example>Authorization is required for this resource</example>
	public string Message { get; set; } = "You don't have permission to access this resource";

}
