using System.Text.Json;

namespace App.API;


/// <summary>Veiklų API</summary>
public static class Veiklos {
	/// <summary>Veiklų statinės reikšmės</summary>
	public static CachedLookup VeiklosVal { get; } = new CachedLookup("Veiklos", ("Stebesenos","lkp_stebesenos"),("Statusas","lkp_statusas"),("RuosimoMedziagos","lkp_ruosimo_medziagos"));

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
