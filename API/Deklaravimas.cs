using System.Text.Json;
using App.Auth;
using G9.Models;

namespace App.API;


/// <summary>Veiklų API</summary>
public static class Veiklos {
	/// <summary>Veiklų statinės reikšmės</summary>
	public static CachedLookup VeiklosVal { get; } = new CachedLookup("Veiklos", ("Stebesenos","lkp_stebesenos"),("Statusas","lkp_statusas"));

	/// <summary>Gauti vartotojo veiklas</summary>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx,CancellationToken ct){
		ctx.Response.ContentType="application/json";
		var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
		using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
		writer.WriteStartObject();
		var rls = ctx.GetUser()?.Roles?.ToArray();
		var gvts = new DBParams(("@gvts", rls));
		writer.WritePropertyName("JA");
		await DBExtensions.PrintArray("SELECT DISTINCT \"ID\",\"Title\",\"Addr\" FROM public.v_gvts_ja WHERE gvts = ANY(@gvts);", gvts, writer, ct);
		writer.WritePropertyName("GVTS");
		await DBExtensions.PrintArray("SELECT * FROM public.v_gvts WHERE \"ID\" = ANY(@gvts);", gvts, writer, ct);
		writer.WritePropertyName("Deklaracijos");
		await DBExtensions.PrintArray("SELECT * FROM public.v_deklar WHERE \"GVTS\" = ANY(@gvts)", gvts, writer, ct, VeiklosVal);
		writer.WriteEndObject();
		await writer.FlushAsync(ct);
	}
}

/// <summary>Rodiklių API</summary>
 public static class Rodikliai {
	/// <summary>Rodiklių statinės reikšmės</summary>
	public static CachedLookup RodikliaiVal { get; } = new CachedLookup("Rodikliai", ("Grupe","lkp_rodikliu_grupes"),("Daznumas","lkp_daznumas"));

	/// <summary>Dažnumo statinės reikšmės</summary>
	public static CachedLookup DaznumasVal { get; } = new CachedLookup("Daznumas", ("Daznumas","lkp_daznumas"),("Laikas","lkp_daznumo_laikas"));

	/// <summary>Gauti visus G9 rodiklius</summary>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	/// <returns></returns>
 	public static async Task List(HttpContext ctx,CancellationToken ct){ 		
		ctx.Response.ContentType="application/json";
		var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
		using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
		writer.WriteStartObject();
		var rls = ctx.GetUser()?.Roles?.ToArray();
		writer.WritePropertyName("Rodikliai");
		//TODO: Cache this
		await DBExtensions.PrintArray("SELECT * FROM public.v_rodikliai;", null, writer, ct, RodikliaiVal);
		writer.WritePropertyName("Daznumas");
		await DBExtensions.PrintArray("SELECT * FROM public.v_daznumas;", null, writer, ct,  DaznumasVal);

		writer.WritePropertyName("Stebesenos");
		writer.WriteStartArray();
		foreach(var i in Veiklos.VeiklosVal["Stebesenos"]){
			if(int.TryParse(i.Key, out var key)){
				writer.WriteStartObject();
				writer.WriteNumber("ID", key);
				writer.WriteString("Pavadinimas", i.Value);
				writer.WritePropertyName("Rodikliai");
				await DBExtensions.PrintList("SELECT stb_rodiklis FROM public.stebesenos WHERE stb_stebesenos=@key ORDER BY stb_rodiklis;",new(("@key",key)),writer,ct);
				writer.WriteEndObject();
			}
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
		await writer.FlushAsync(ct);
 	}

}


/// <summary>Reikšmių API</summary>
public static class Reiksmes{
	/// <summary>Gauti suvestas rodiklių reikšmes</summary>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	/// <param name="deklaracija">Geriamo vandens tiekimo sistems</param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx, long deklaracija,CancellationToken ct){
		var gvts = await new DBExec("SELECT dkl_gvts FROM deklaravimas WHERE dkl_id=@id;", "@id", deklaracija).ExecuteScalar<long>(ct);
		if(gvts>0){
			if(ctx.GetUser()?.Roles?.Contains(gvts) == true) await PrintRodikl(ctx,deklaracija,ct);
			else Error.E403(ctx,true);
		} else Error.E404(ctx,true);
	}
	
	private static async Task PrintRodikl(HttpContext ctx, long deklaracija, CancellationToken ct){
		ctx.Response.ContentType="application/json";
		var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
		using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
		await DBExtensions.PrintArray("SELECT * FROM public.v_reiksmes where \"Deklaracija\"=@id;", new(("@ID",deklaracija)), writer, ct);
		await writer.FlushAsync(ct);
		return;
	}
// 	public static async Task Set(HttpContext ctx,CancellationToken ct, long gvts, long metai){
// 		ctx.Response.ContentType="application/json";
// 		await ctx.Response.WriteAsync("{}",ct);
// 	}
// 	public static async Task Del(HttpContext ctx,CancellationToken ct, long gvts, long metai, int id){
// 		ctx.Response.ContentType="application/json";
// 		await ctx.Response.WriteAsync("{}",ct);
// 	}
}


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
			var stat = await new DBExec("SELECT dkl_status FROM public.deklaravimas WHERE dkl_gvts=@gvts and dkl_metai=@metai;", param).ExecuteScalar<int?>(ct);
			if(stat is null) Error.E404(ctx, true);
			else if(stat==3) Error.E422(ctx,true,$"Negalima keisti jau deklaruotų duomenų");
			else {
				await new DBExec("UPDATE public.deklaravimas SET dkl_kiekis=@kiekis, dkl_vartot=@vartot WHERE dkl_gvts=@gvts and dkl_metai=@metai;", param).Execute(ct);
				await PrintDeklar(ctx,gvts,metai,ct);
			}
		} else Error.E403(ctx,true);
	}
}
