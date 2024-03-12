using System.Text.Json;
using App.Auth;
using G9.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.API;

/// <summary>Vartotojo teisės ir prieigos</summary>
public static class Prieigos {
	private static CachedLookup PrieigosApiVal { get; } = new CachedLookup("APIKey", ("Stebesenos","lkp_stebesenos"), ("Statusas","lkp_statusas"));


	/// <summary>Gauti administruojamų GVTS deleguotus asmenis</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	public static async Task Get(HttpContext ctx,CancellationToken ct){
		var rle = ctx.GetUser()?.Admin;
		if(rle?.Count>0){
			ctx.Response.ContentType="application/json";
			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
			writer.WriteStartObject();
			var gvts = new DBParams(("@gvts", rle.ToArray()));
			writer.WritePropertyName("GVTS");
			await DBExtensions.PrintArray("SELECT * FROM g9.v_gvts WHERE \"ID\" = ANY(@gvts);", gvts, writer, ct);
			writer.WritePropertyName("Users");
			await DBExtensions.PrintArray("SELECT * FROM g9.gvts_users(@gvts);", gvts, writer, ct);
			writer.WriteEndObject();
			await writer.FlushAsync(ct);
		} else Error.E403(ctx,true);
	}

	/// <summary>Pridėti deleguojamą asmenį</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="user">Deleguojamas asmuo</param>
	public static async Task Set(HttpContext ctx, long gvts, DelegavimasSet user, CancellationToken ct){
		var usr = ctx.GetUser();
		if(usr?.Admin?.Contains(gvts) == true) {
			ctx.Response.ContentType="application/json";
			var guid = await new DBExec("SELECT app.user_check(@ak);","@ak",user.AK).ExecuteScalar<Guid?>(ct);
			if(usr.ID==guid) Error.E422(ctx,true,"Vartotojas negali deleguoti savęs.");
			else {
				if(guid is null){
					using var db = new DBExec("SELECT * FROM app.user_add(@ak,@fname,@lname,null,null);",("@ak",user.AK),("@fname",user.FName),("@lname",user.LName));
					using var rdr = await db.GetReader(ct);
					if(await rdr.ReadAsync(ct)){
						if(await rdr.IsDBNullAsync(0,ct)) { Error.E422(ctx,true,rdr.GetStringN(1)??"Nežinoma klaida"); return; }
						guid = rdr.GetGuid(0);
					} else { Error.E500(ctx,true,"Neįmanoma sukurti vartotojo"); return; }
				}
				if(await AddUser(gvts,guid,user.Admin,ct) is null) {
					var rle = await new DBExec($"SELECT g9.gvts_group(@gvts,@adm);",("@gvts",gvts),("@adm",user.Admin)).ExecuteScalar<string?>(ct);
					if(rle is not null) { Error.E422(ctx,true,rle); return; }
				}
				await AddUser(gvts,guid,user.Admin,ct);
				ctx.Response.StatusCode=204;
				await ctx.Response.CompleteAsync();
			}
		}
		else Error.E403(ctx,true);
	}
	private static async Task<long?> AddUser(long gvts, Guid? guid, bool adm, CancellationToken ct) =>
		await new DBExec($"SELECT id FROM app.user_role_add(@usr,'g9.{gvts}{(adm?".admin":"")}');",("@usr",guid),("@role",gvts)).ExecuteScalar<long?>(ct);
	
	
	/// <summary>Pašalinti deleguotą asmenį</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="user">Vartotojo identifikatorius</param>
	public static async Task Del(HttpContext ctx, long gvts, Guid user, CancellationToken ct){
		var usr = ctx.GetUser();
		if(usr?.Admin?.Contains(gvts) == true) {
			ctx.Response.ContentType="application/json";
			if(usr.ID==user) Error.E422(ctx,true,"Vartotojas negali pašalinti pats save.");
			else {
				new DBExec($"SELECT app.user_role_del(@usr,'g9.{gvts}'), app.user_role_del(@usr,'g9.{gvts}.admin');","@usr",user).Execute();
			}
			ctx.Response.StatusCode=204;
			await ctx.Response.CompleteAsync();
		}
		else Error.E403(ctx,true);
	}




	/// <summary>Gauti visus API raktus priskirtus GVTS</summary>
	/// <param name="ctx"></param>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="ct"></param>
	/// <returns></returns>
	public static async Task GetKeys(HttpContext ctx, long gvts, CancellationToken ct) {
		var usr = ctx.GetUser();
		if(usr?.Admin?.Contains(gvts) == true) {
			ctx.Response.ContentType="application/json";
			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
			writer.WriteStartObject();			
			var prm = new DBParams(("@gvts", gvts));
			writer.WritePropertyName("Deklaracijos");
			await DBExtensions.PrintArray("SELECT \"ID\",\"Metai\",\"Stebesenos\",\"Statusas\" FROM g9.v_deklar WHERE \"GVTS\"=@gvts and \"Statusas\" in (1,2);", prm, writer, ct, PrieigosApiVal);
			writer.WritePropertyName("Raktai");
			await DBExtensions.PrintArray("SELECT * FROM g9.v_deklar_keys WHERE \"GVTS\"=@gvts;", prm, writer, ct);
			writer.WriteEndObject();
			await writer.FlushAsync(ct);
		}
		else Error.E403(ctx,true);
	}

	/// <summary>Trinti API raktą</summary>
	/// <param name="ctx"></param>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="id">API Rakto ID</param>
	/// <param name="ct"></param>
	/// <returns></returns>
	public static async Task DelKey(HttpContext ctx, long gvts, int deklaracija, Guid id, CancellationToken ct) {
		var usr = ctx.GetUser();
		if(usr?.Admin?.Contains(gvts) == true) {
			var ret = await new DBExec("SELECT app.api_key_del(@id,@gvts,@deklar)",("@id",id),("@gvts",gvts),("@deklar",deklaracija)).ExecuteScalar<bool>(ct);
			ctx.Response.ContentType="application/json";
			await ctx.Response.WriteAsJsonAsync(new APIKeyDel(){ Ištrinta=ret }, cancellationToken: ct);
		}
		else Error.E403(ctx,true);
	}


	/// <summary>Sukurti API raktą deklaracijai</summary>
	/// <param name="ctx"></param>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="dt">Rakto informacija</param>
	/// <param name="ct"></param>
	/// <returns></returns>
	public static async Task AddKey(HttpContext ctx, long gvts, [FromBody] APIKeyAdd dt, CancellationToken ct) {
		var usr = ctx.GetUser();
		if(usr?.Admin?.Contains(gvts) == true) {
			var key = Session.RandomStr(Config.GetInt("APIKey","Length",64));
			
			var date = DateOnly.FromDateTime(DateTime.UtcNow);
			if(dt.GaliojaIki<date){ dt.GaliojaIki=date.AddDays(1); }
			else {
				date=date.AddDays(Config.GetInt("APIKey","MaxDate",420));
				if(dt.GaliojaIki is null || dt.GaliojaIki > date) dt.GaliojaIki=date;
			}

			if(dt.GaliojaIki>date) dt.GaliojaIki=date;
			using var db = new DBExec("SELECT id,key,deklar,exp FROM app.api_key_add(@key,@gvts,@deklar,@exp,@usr)",("@key",key),("@gvts",gvts),("@deklar",dt.Deklaracija),("@exp",dt.GaliojaIki),("@usr",usr.ID));
			using var rdr = await db.GetReader(ct);
			ctx.Response.ContentType="application/json";
			if(await rdr.ReadAsync(ct)){
				await ctx.Response.WriteAsJsonAsync(new APIKeyData(){ RaktoID=rdr.GetGuid(0), Raktas=rdr.GetStringN(1), Deklaracija=rdr.GetIntN(2)??0, GaliojaIki=DateOnly.FromDateTime(rdr.GetDateTimeN(3)??DateTime.Today), GVTS=gvts },ct);
			} else Error.E422(ctx,true,"API autorizacijos raktas nabuvo sukirtas");
		}
		else Error.E403(ctx,true);
	}

}