using System.Text.Json;
using G9.Models;

namespace App.API;


public static class Veiklos {
	public static async Task Get(HttpContext ctx,CancellationToken ct){
		ctx.Response.ContentType="application/json";
		//await ctx.Response.WriteAsync("[{\"name\":\"Test Function\"}]",ct);


//		var connString = "User ID=postgres;Password=postgres;Server=localhost;Port=5002;Database=postgres;Integrated Security=true;Pooling=true;";

//		await using var conn = new NpgsqlConnection(connString);
//		await conn.OpenAsync(ct);

		var options = new JsonWriterOptions{ Indented = false }; //todo: if debug

		using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);


		//using var cmd = new NpgsqlCommand("SELECT id,name,title,gen_random_uuid() as uuid, now() as time FROM test", conn);

//		using var reader = await cmd.ExecuteReaderAsync(ct);

		

		writer.WriteStartObject();
		writer.WritePropertyName("Data");
		writer.WriteStartArray();
//		await Loop(reader,writer,ct);
		writer.WriteEndArray();
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


public static class Deklaravimas{
	public static async Task Get(HttpContext ctx,CancellationToken ct, long gvts, long metai){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync("{}",ct);
	}
	public static async Task Set(HttpContext ctx,CancellationToken ct, long gvts, long metai, Deklaruoti dcl){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync("{}",ct);
	}
}