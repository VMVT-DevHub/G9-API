using System.Data.Common;
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

// public static class Rodikliai {
// 	public static async Task List(HttpContext ctx,CancellationToken ct){
// 		ctx.Response.ContentType="application/json";
// 		await ctx.Response.WriteAsync("{}",ct);
// 	}

// 	public static async Task Get(HttpContext ctx,CancellationToken ct, long gvts, long metai){
// 		ctx.Response.ContentType="application/json";
// 		await ctx.Response.WriteAsync("{}",ct);
// 	}
// 	public static async Task Set(HttpContext ctx,CancellationToken ct, long gvts, long metai){
// 		ctx.Response.ContentType="application/json";
// 		await ctx.Response.WriteAsync("{}",ct);
// 	}
// 	public static async Task Del(HttpContext ctx,CancellationToken ct, long gvts, long metai, int id){
// 		ctx.Response.ContentType="application/json";
// 		await ctx.Response.WriteAsync("{}",ct);
// 	}
// }


/// <summary>Deklaravimo API</summary>
public static class Deklaravimas {
	/// <summary>Gauti informaciją apie deklaruojamus metus</summary>
	/// <param name="ctx"></param><param name="ct"></param>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="metai">Deklaruojami metai</param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx, long gvts, int metai, CancellationToken ct){
		if(ctx.GetUser()?.Roles?.Contains(gvts) == true) await PrintDeklar(ctx,gvts,metai,ct);
		else Error.E403(ctx,true);
	}

	private static async Task PrintDeklar(HttpContext ctx, long gvts, int metai, CancellationToken ct){
		using var db = new DBExec("SELECT * FROM public.v_deklar WHERE \"GVTS\"=@gvts and \"Metai\"=@metai;", ("@gvts",gvts), ("@metai",metai));
		using var rdr = await db.GetReaderAsync(ct);
		if(rdr.HasRows){
			ctx.Response.ContentType="application/json";
			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
			writer.WriteStartObject();
				await DBExtensions.GetObject(rdr,writer,ct);
			writer.WriteEndObject();
			await writer.FlushAsync(ct);
			return;
		} else Error.E404(ctx,true);
	}

	/// <summary>Deklaravimo pagrindinės informacijos pildymas</summary>
	/// <param name="ctx"></param>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="metai">Deklaruojami metai</param>
	/// <param name="dcl">Deklaruojami duomenys</param>
	/// <param name="ct"></param><returns></returns>
	public static async Task Set(HttpContext ctx, long gvts, int metai, DeklaravimasSet dcl, CancellationToken ct){
		ctx.Response.ContentType="application/json";
		if(ctx.GetUser()?.Roles?.Contains(gvts) == true){
			var param = new DBParams(("@gvts",gvts), ("@metai",metai), ("@kiekis",dcl.Kiekis),("@vartot",dcl.Vartotojai));
			var stat = await new DBExec("SELECT dkl_status FROM public.deklaravimas WHERE dkl_gvts=@gvts and dkl_metai=@metai;", param).ExecuteScalar<string>(ct);

			if(string.IsNullOrEmpty(stat)) Error.E404(ctx, true);
			else if(stat=="Deklaruoti") Error.E422(ctx,true,$"Negalima keisti jau deklaruotų duomenų");
			else {
				await new DBExec("UPDATE public.deklaravimas SET dkl_kiekis=@kiekis, dkl_vartot=@vartot WHERE dkl_gvts=@gvts and dkl_metai=@metai;", param).Execute(ct);
				await PrintDeklar(ctx,gvts,metai,ct);
			}
		} else Error.E403(ctx,true);
	}
}

