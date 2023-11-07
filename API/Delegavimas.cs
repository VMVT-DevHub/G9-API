using System.Text.Json;
using G9.Models;

namespace App.API;


public static class Delegavimas {
	public static async Task Get(HttpContext ctx,CancellationToken ct){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync("{}",ct);
	}

	public static async Task Set(HttpContext ctx, long gvts, Asmuo asmuo, CancellationToken ct){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync(JsonSerializer.Serialize(asmuo),ct);
	}
	
	public static async Task Del(HttpContext ctx, long gvts, long ak, CancellationToken ct){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync($"{{\"id\":\"{gvts}\",\"ak\":\"{ak}\"}}",ct);
	}
}