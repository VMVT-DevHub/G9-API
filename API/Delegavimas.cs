using System.Text.Json;
using G9.Models;

namespace App.API;

/// <summary>Vartotojo teisių delegavimas</summary>
public static class Delegavimas {
	/// <summary>Gauti administruojamų GVTS deleguotus asmenis</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	public static async Task Get(HttpContext ctx,CancellationToken ct){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync("{}",ct);
	}

	/// <summary>Pridėti deleguojamą asmenį</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="asmuo">Deleguojamas asmuo</param>
	public static async Task Set(HttpContext ctx, long gvts, DelegavimasSet asmuo, CancellationToken ct){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync(JsonSerializer.Serialize(asmuo),ct);
	}
	
	/// <summary>Pašalinti deleguotą asmenį</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="id">Vartotojo identifikatorius</param>
	public static async Task Del(HttpContext ctx, long gvts, Guid id, CancellationToken ct){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync($"{{\"id\":\"{gvts}\",\"ak\":\"{id}\"}}",ct);
	}
}