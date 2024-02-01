using System.Text.Json;
using App.Auth;
using G9.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.API;

/// <summary>Reikšmių API</summary>
public static class Reiksmes {
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
	/// <summary>Įrašyti rodiklio reikšmę</summary>
	/// <param name="ctx"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="data">Rodiklio reikšmių masyvas</param>
	/// <param name="ct"></param>
	/// <returns></returns>
 	public static async Task Set(HttpContext ctx, long deklaracija, List<RodiklisSet> data ,CancellationToken ct){
		using var db = new DBExec("SELECT dkl_gvts, dkl_status, dkl_metai FROM deklaravimas WHERE dkl_id=@id;","@id",deklaracija);
		using var rdr = await db.GetReader(ct);
		if(rdr.Read()){
			var usr = ctx.GetUser();
			if(usr?.ID is not null && usr.Roles?.Contains(rdr.GetInt64(0)) == true){			
				if(rdr.GetInt32(1)==3) Error.E422(ctx,true,$"Negalima keisti jau deklaruotų duomenų");
				else {
					var flush = Config.GetInt("DBBatch","Reiksmes",100);
					var cnt = flush;
					var metai = rdr.GetInt32(2);
					var rod = new List<int>();
					var dte = new List<DateOnly>();
					var val = new List<double>();
					foreach(var i in data){
						if(i.Data.Year!=metai) { Error.E422(ctx,true,"Įrašo data neatitinka deklaruojamų metų"); return; }
						rod.Add(i.Rodiklis); dte.Add(i.Data); val.Add(i.Reiksme);
						if(cnt--<1){ await WriteReiksmes(deklaracija,usr.ID,rod,dte,val,ct); cnt=flush;  }
					}
					await WriteReiksmes(deklaracija,usr.ID,rod,dte,val,ct);
					//TODO: Log stuff;
					ctx.Response.StatusCode=204;
					await ctx.Response.CompleteAsync();
				}
			} else Error.E403(ctx,true);
		} else Error.E404(ctx,true);
 	}

	private static async Task<int> WriteReiksmes(long deklaracija, Guid? user, List<int> rodk, List<DateOnly> date, List<double> reiksme, CancellationToken ct){
		var ret = 0;
		if(rodk.Count>0) {
			ret = await new DBExec("INSERT into public.reiksmes (rks_deklar,rks_user,rks_rodiklis,rks_date,rks_reiksme) SELECT @id, @usr, t.* FROM unnest(@rod,@date,@val) t;",
			("@id",deklaracija),("@usr",user),("@rod",rodk),("@date",date),("@val",reiksme)).Execute(ct);
		}
		rodk.Clear(); date.Clear(); reiksme.Clear();
		return ret;
	}

	/// <summary>Rodiklio reikšmių trynimas</summary>
	/// <param name="ctx"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="data">Įvedimo identifikatorių masyvas</param>
	/// <param name="ct"></param>
	/// <returns></returns>
 	public static async Task Del(HttpContext ctx, long deklaracija, [FromBody] List<long> data, CancellationToken ct){
		using var db = new DBExec("SELECT dkl_gvts, dkl_status, dkl_metai FROM deklaravimas WHERE dkl_id=@id;","@id",deklaracija);
		using var rdr = await db.GetReader(ct);
		if(rdr.Read()){
			if(ctx.GetUser()?.Roles?.Contains(rdr.GetInt64(0)) == true){			
				if(rdr.GetInt32(1)==3) Error.E422(ctx,true,$"Negalima keisti jau deklaruotų duomenų");
				else {
					 await new DBExec("DELETE FROM public.reiksmes WHERE rks_deklar=@id and rks_id = ANY(@lst)",("@id",deklaracija),("@lst",data)).Execute(ct);
					//TODO: Log stuff;
					ctx.Response.StatusCode=204; await ctx.Response.CompleteAsync();
				}
			} else Error.E403(ctx,true);
		} else Error.E404(ctx,true);
 	}
}
