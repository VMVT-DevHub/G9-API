using System.Text.Json;
using App.Auth;
using G9.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.API;

/// <summary>Deklaracijų API</summary>
public static class Deklaracija {
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
		using var rdr = await db.GetReader(ct);
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
	public static async Task Set(HttpContext ctx, long gvts, int metai, [FromBody] DeklaracijaSet dcl, CancellationToken ct){
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