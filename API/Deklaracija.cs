using System.Text.Json;
using App.Auth;
using G9.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.API;

/// <summary>Deklaracijų API</summary>
public static class Deklaracija {
	/// <summary>GVTS deklaracijos metams</summary>
	/// <param name="ctx"></param><param name="ct"></param>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="metai">Deklaruojami metai</param>
	/// <returns></returns>
	public static async Task GetYear(HttpContext ctx, long gvts, int metai, CancellationToken ct){
		if(ctx.GetUser()?.Roles?.Contains(gvts) == true) {
			ctx.Response.ContentType="application/json";
			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
			await DBExtensions.PrintArray("SELECT * FROM g9.v_deklar WHERE \"GVTS\"=@gvts and \"Metai\"=@metai;", new(("@gvts",gvts), ("@metai",metai)), writer, ct, Veiklos.VeiklosVal);
			await writer.FlushAsync(ct);
		}
		else Error.E403(ctx,true);
	}

	/// <summary>Visos GVTS deklaracijos</summary>
	/// <param name="ctx"></param><param name="ct"></param>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <returns></returns>
	public static async Task GetAll(HttpContext ctx, long gvts, CancellationToken ct){
		if(ctx.GetUser()?.Roles?.Contains(gvts) == true) {
			ctx.Response.ContentType="application/json";
			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
			await DBExtensions.PrintArray("SELECT * FROM g9.v_deklar WHERE \"GVTS\"=@gvts;", new(("@gvts",gvts)), writer, ct, Veiklos.VeiklosVal);
			await writer.FlushAsync(ct);
		}
		else Error.E403(ctx,true);
	}

	/// <summary>Deklaracija pagal ID</summary>
	/// <param name="ctx"></param><param name="ct"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <returns></returns>
	public static async Task GetOne(HttpContext ctx, long deklaracija, CancellationToken ct){
		var gvts = new DBExec("SELECT dkl_gvts FROM g9.deklaravimas WHERE dkl_id=@id;",("@id",deklaracija)).ExecuteScalar<long>();
		if(ctx.GetUser()?.Roles?.Contains(gvts) == true) {
			ctx.Response.ContentType="application/json";
			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
			await DBExtensions.PrintArray("SELECT * FROM g9.v_deklar WHERE \"ID\"=@id;", new(("@id",deklaracija)), writer, ct, Veiklos.VeiklosVal);
			await writer.FlushAsync(ct);
		}
		else Error.E403(ctx,true);
	}


	/// <summary>Deklaravimo pagrindinės informacijos pildymas</summary>
	/// <param name="ctx"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="dcl">Deklaruojami duomenys</param>
	/// <param name="ct"></param><returns></returns>
	public static async Task Set(HttpContext ctx, long deklaracija, [FromBody] DeklaracijaSet dcl, CancellationToken ct){
		using var db = new DBExec("SELECT dkl_gvts,dkl_status FROM g9.deklaravimas WHERE dkl_id=@id;",("@id",deklaracija));
		var rdr = await db.GetReader(ct);
		if(rdr.HasRows & rdr.Read()){
			var gvts=rdr.GetLongN(0);
			var usr=ctx.GetUser();
			if(usr?.Roles?.Contains(gvts??0) == true) {
				var stat = rdr.GetIntN(1);
				if(stat is null) Error.E404(ctx, true);
				else if(stat==3) Error.E422(ctx, true, $"Negalima keisti jau deklaruotų duomenų");
				else {
					var param = new DBParams(("@id",deklaracija), ("@kiekis",dcl.Kiekis),("@vartot",dcl.Vartotojai),("@medziag",dcl.RuosimoMedziagos),
						("@name",usr.FullName),("@usr",usr.Id));
					await new DBExec("UPDATE g9.deklaravimas SET dkl_kiekis=@kiekis, dkl_vartot=@vartot, dkl_medziagos=@medziag, "+
						"dkl_modif_date=timezone('utc',now()), dkl_modif_user=@name, dkl_modif_user_id=@usr WHERE dkl_id=@id;", param).Execute(ct);
					
					ctx.Response.ContentType="application/json";
					var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
					using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
					await DBExtensions.PrintArray("SELECT * FROM g9.v_deklar WHERE \"ID\"=@id;", new(("@id",deklaracija)), writer, ct, Veiklos.VeiklosVal);
					await writer.FlushAsync(ct);
				}
			}
			else Error.E403(ctx,true);
		} else Error.E404(ctx,true);
	}

	
	/// <summary>Gauti suvedamų rodikliu sąrašą</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="deklaracija">Deklaracijos ID</param>
	public static async Task GetSuvesti(HttpContext ctx, int deklaracija, CancellationToken ct){
		if(await Deklaravimas.Validate(ctx,deklaracija,ct,true)) {
			ctx.Response.ContentType="application/json";
			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
			await DBExtensions.PrintArray("SELECT * FROM g9.valid_suvesti(@deklar);", new DBParams(("@deklar", deklaracija)), writer, ct);
			await writer.FlushAsync(ct);
		}
	}

}