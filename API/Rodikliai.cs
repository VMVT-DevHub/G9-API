using System.Runtime.CompilerServices;
using System.Text.Json;
using App.Auth;
using G9.Models;
using Microsoft.AspNetCore.Mvc;

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
		await DBExtensions.PrintArray("SELECT * FROM g9.v_rodikliai;", null, writer, ct, RodikliaiVal);
		writer.WritePropertyName("Daznumas");
		await DBExtensions.PrintArray("SELECT * FROM g9.v_daznumas;", null, writer, ct,  DaznumasVal);

		writer.WritePropertyName("Stebesenos");
		writer.WriteStartArray();
		foreach(var i in Veiklos.VeiklosVal["Stebesenos"]){
			if(int.TryParse(i.Key, out var key)){
				writer.WriteStartObject();
				writer.WriteNumber("ID", key);
				writer.WriteString("Pavadinimas", i.Value);
				writer.WritePropertyName("Rodikliai");
				await DBExtensions.PrintList("SELECT stb_rodiklis FROM g9.stebesenos WHERE stb_stebesenos=@key ORDER BY stb_rodiklis;",new(("@key",key)),writer,ct);
				writer.WriteEndObject();
			}
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
		await writer.FlushAsync(ct);
 	}

}




/// <summary>Rodiklio informacija</summary>
public class Rodiklis {
	/// <summary>Rodiklio unikalus numeris</summary>
	public int ID { get; set; }
	/// <summary>Rodiklio grupė</summary>
	public int Grupe { get; set; }
	/// <summary>Rodiklio EU numeris</summary>
	public string? Kodas { get; set; }
	/// <summary>Rodiklio pavadinimas</summary>
	public string? Pavadinimas { get; set; }
}

