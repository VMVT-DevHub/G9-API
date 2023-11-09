namespace App.Auth;


/// <summary>Autorizacijos reikalavimo funkcijos</summary>
public static class Require {
	/// <summary>Privaloma autorizacija</summary>
	/// <param name="ctx"></param><param name="next"></param><returns></returns>
	public static async ValueTask<object?> Login(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next) 
		=> ctx.HttpContext.GetAuth() ? await next(ctx) : E401.Response;

	/// <summary>Vartotojas turi turėti bent vieną rolę</summary>
	/// <param name="ctx"></param><param name="next"></param><returns></returns>
	public static async ValueTask<object?> Role (EndpointFilterInvocationContext ctx, EndpointFilterDelegate next){
		if (ctx.HttpContext.GetAuth()){
			//todo: search roles
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
