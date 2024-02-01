using System.Text.Json;
using App.Auth;

namespace App.API;

/// <summary>Rodiklių API</summary>
 public static class Rodikliai {
	/// <summary>Rodiklių statinės reikšmės</summary>
	public static CachedLookup RodikliaiVal { get; } = new CachedLookup("Rodikliai", ("Grupe","lkp_rodikliu_grupes"),("GrupesValidacija","lkp_rodikliu_validacija"),("Daznumas","lkp_daznumas"));

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

	/// <summary>Gauti suvestus deklaracijos rodiklius</summary>
	/// <param name="ctx"></param><param name="ct"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx, CancellationToken ct, long deklaracija){ 
		if(ApiCheck(ctx, deklaracija)){
			ctx.Response.ContentType="application/json";			
			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
			await DBExtensions.PrintArray("SELECT \"ID\",\"Suvedimas\",\"Kodas\",\"Data\",\"Reiksme\" FROM public.v_rodikliai_suvedimas WHERE \"Deklaracija\"=@id;", new(("@id",deklaracija)), writer, ct);
			await writer.FlushAsync(ct);
		}
	}

	/// <summary>Įvesti rodiklius deklaracijai</summary>
	/// <param name="ctx"></param><param name="ct"></param>
	/// <param name="deklaracija"> Deklaracijos ID</param>
	/// <returns></returns>
	public static async Task Set(HttpContext ctx, CancellationToken ct, long deklaracija){ 	}

	/// <summary>Ištrinti suvestus deklaracijos rodiklius</summary>
	/// <param name="ctx"></param><param name="ct"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <returns></returns>
	public static async Task Del(HttpContext ctx, CancellationToken ct, long deklaracija){ 	}

	private static bool ApiCheck(HttpContext ctx, long deklaracija){
		var api = ctx.GetAPI();
		if(api is not null && api.Deklaracija==deklaracija) return true;
		Error.E403(ctx,true); return false;
	}
}
