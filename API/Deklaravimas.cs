using System.Text.Json;
using App.Auth;
using G9.Models;

namespace App.API;


/// <summary>Veiklų API</summary>
public static class Veiklos {
	/// <summary>Gauti vartotojo veiklas</summary>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx,CancellationToken ct){
		ctx.Response.ContentType="application/json";
		var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
		using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
		writer.WriteStartObject();
		var rls = ctx.GetUser()?.Roles;
		writer.WritePropertyName("JA");
		await DBExtensions.PrintArray("SELECT DISTINCT \"ID\",\"Title\",\"Addr\" FROM public.v_gvts_ja WHERE gvts = ANY(@gvts);", new(("@gvts", rls)), writer, ct);
		writer.WritePropertyName("GVTS");
		await DBExtensions.PrintArray("SELECT * FROM public.v_gvts WHERE \"ID\" = ANY(@gvts);", new(("@gvts", rls)), writer, ct);
		writer.WritePropertyName("Deklaracijos");
		await DBExtensions.PrintArray("SELECT * FROM public.v_deklar WHERE \"GVTS\" = ANY(@gvts)", new(("@gvts", rls)), writer, ct);
		writer.WriteEndObject();
		await writer.FlushAsync(ct);
	}
}

public static class Rodikliai {
	public static async Task List(HttpContext ctx,CancellationToken ct){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync("{}",ct);
	}

	public static async Task Get(HttpContext ctx,CancellationToken ct, long gvts, long metai){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync("{}",ct);
	}
	public static async Task Set(HttpContext ctx,CancellationToken ct, long gvts, long metai){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync("{}",ct);
	}
	public static async Task Del(HttpContext ctx,CancellationToken ct, long gvts, long metai, int id){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync("{}",ct);
	}


}


/// <summary>Deklaravimo API</summary>
public static class Deklaravimas {
	/// <summary>Gauti informaciją apie deklaruojamus metus</summary>
	/// <param name="ctx"></param><param name="ct"></param>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="metai">Deklaruojami metai</param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx, CancellationToken ct, long gvts, int metai){
		if(ctx.GetUser()?.Roles?.Contains(gvts) == true){
			ctx.Response.ContentType="application/json";
			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
			writer.WriteStartObject();
			
			writer.WritePropertyName("JA");

			// await DBExtensions.PrintArray("SELECT DISTINCT \"ID\",\"Title\",\"Addr\" FROM public.v_gvts_ja WHERE gvts = ANY(@gvts);", new(("@gvts", rls)), writer, ct);
			// writer.WritePropertyName("GVTS");
			// await DBExtensions.PrintArray("SELECT * FROM public.v_gvts WHERE \"ID\" = ANY(@gvts);", new(("@gvts", rls)), writer, ct);
			// writer.WritePropertyName("Deklaracijos");
			// await DBExtensions.PrintArray("SELECT * FROM public.v_deklar WHERE \"GVTS\" = ANY(@gvts)", new(("@gvts", rls)), writer, ct);
			// writer.WriteEndObject();
			await writer.FlushAsync(ct);
		}
		Error.E403(ctx,true);
	}
	public static async Task Set(HttpContext ctx,CancellationToken ct, long gvts, int metai, Deklaruoti dcl){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync("{}",ct);
	}
}

