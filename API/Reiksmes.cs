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




/// <summary>
/// Rodiklių suvedimo automatizavimo API
/// </summary>
public class IntegracijosAPI_v1 {

	private static CachedLookup<string,Rodiklis> RodikliaiList { get; } = new ("Rodikliai", (dict)=>{
		using var db = new DBExec("SELECT rod_id,rod_grupe,rod_kodas,rod_rodiklis FROM public.rodikliai");
		using var rdr = db.GetReader();
		while(rdr.Read()){
			var kod = rdr.GetStringN(2);
			if(!string.IsNullOrEmpty(kod))
				dict[kod]=new(){ ID=rdr.GetIntN(0)??0, Grupe=rdr.GetIntN(1)??0, Kodas=kod, Pavadinimas=rdr.GetStringN(3) };
		}
	});



	/// <summary>Gauti visas suvestas deklaracijos rodiklių reikšmes</summary>
	/// <param name="ctx"></param><param name="ct"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="rodiklis">Rodiklio kodas</param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx, int deklaracija, CancellationToken ct, [FromQuery] string? rodiklis=null){ 
		if(ApiCheck(ctx, deklaracija) is not null){
			if(string.IsNullOrEmpty(rodiklis)){
				ctx.Response.ContentType="application/json";			
				var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
				using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
				await DBExtensions.PrintArray("SELECT \"ID\",\"Suvedimas\",\"Kodas\",\"Data\",\"Reiksme\" FROM public.v_rodikliai_suvedimas WHERE \"Deklaracija\"=@deklar;", new(("@deklar",deklaracija)), writer, ct);
				await writer.FlushAsync(ct);
			} else {
				if(RodikliaiList.Refresh().TryGetValue(rodiklis, out var rdk)){
					ctx.Response.ContentType="application/json";			
					var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
					using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
					await DBExtensions.PrintArray("SELECT rks_id \"ID\", rks_suvedimas \"Suvedimas\", @code \"Kodas\", rks_date \"Data\", rks_reiksme \"Reiksme\" FROM public.reiksmes WHERE rks_deklar=@deklar and rks_rodiklis=@id", 
						new(("@deklar",deklaracija),("@id",rdk.ID),("@code",rdk.Kodas)), writer, ct);
					await writer.FlushAsync(ct);
				} else Error.E404(ctx,true);
			}
		}
	}

	/// <summary>Įvesti rodiklių reikšmes deklaracijai</summary>
	/// <param name="ctx"></param><param name="ct"></param>
	/// <param name="deklaracija"> Deklaracijos ID</param>
	/// <param name="data">Rodilių duomenys</param>
	/// <returns></returns>
	public static async Task Set(HttpContext ctx, int deklaracija, List<object[]> data, CancellationToken ct) {
		var api = ApiCheck(ctx, deklaracija);
		if(api is not null){
			var rdl = RodikliaiList.Refresh();
			using var db = new DBBatch("reiksmes", ["rks_rodiklis","rks_date","rks_reiksme","rks_deklar","rks_suvedimas","rks_user"]);
			
			var sk = await db.ExecuteScalar<long>("INSERT INTO public.suvedimai (rsv_deklar,rsv_type,rsv_user) VALUES (@deklar,'3',@user) RETURNING rsv_id;",ct, ("@deklar",deklaracija), ("@user",api.ID));
			
			var cnt = data.Count;
			for(var i=0; i<cnt ; i++){
				var j = data[i];
				var rod = j[0]?.ToString();
				if(!string.IsNullOrWhiteSpace(rod)){
					if(rdl.TryGetValue(rod,out var rdk)){
						if(DateOnly.TryParse(j[1]?.ToString(), out var dte)){
							if(dte.Year==api.Metai){
								//check date>now;
								if(double.TryParse(j[2]?.ToString(), out var rks)){
									await db.Add(ct, rdk.ID,dte,rks,deklaracija,sk,api.ID);
								} else { Error.E422(ctx,true,$"Reikšmė negalima {i+1}: {rod}, {dte}, {j[2]}"); return; }
							} else { Error.E422(ctx,true,$"Neteisingi deklaravimo metai {i+1}: {rod}, {dte}, {j[2]}"); return; }
						} else { Error.E422(ctx,true,$"Neteisinga {i+1}: {rod}, {j[1]}, {j[2]}"); return; }
					} else { Error.E422(ctx,true,$"Rodiklis nerastas {i+1}: {j[0]}, {j[1]}, {j[2]}"); return; }
				}
			}
			if(db.Total>0) { 
				await db.Commit(ct,true);
				//TODO: Log
			}
			ctx.Response.ContentType="application/json";	
			await ctx.Response.WriteAsJsonAsync(new ReiksmiuSuvedimasResult(){ Deklaracija=deklaracija, Reiksmes=db.Total, Suvedimas=sk},ct);
		}
	}

	/// <summary>Ištrinti deklaracijos rodiklio reikšmę</summary>
	/// <param name="ctx"></param><param name="ct"></param><param name="deklaracija">Deklaracijos ID</param><param name="rodiklis">Rodiklio reikšmės ID</param><param name="suvedimas">Reikšmių suvedimo ID</param><returns></returns>
	public static async Task Del(HttpContext ctx, int deklaracija, CancellationToken ct, long? rodiklis=null, long? suvedimas=null) {
		var api = ApiCheck(ctx, deklaracija);
		if(api is not null){
			int del = 0;
			lock(api){
				if(ct.IsCancellationRequested) return;
				if(rodiklis>0){					
					del = new DBExec("DELETE FROM public.reiksmes WHERE rks_deklar=@deklar and rks_id=@id",("@deklar",deklaracija),("@id",rodiklis)).Execute();
				}
				else if(suvedimas>0){
					del = new DBExec("DELETE FROM public.reiksmes WHERE rks_deklar=@deklar and rks_suvedimas=@id",("@deklar",deklaracija),("@id",suvedimas)).Execute();
				}
				else { Error.E404(ctx,true); return;}
				Thread.Sleep(200);
			}
			await ctx.Response.WriteAsJsonAsync(new ReiksmiuTrynimasResult(){ Deklaracija=deklaracija, Istrinta=del},ct);
			//TODO: Log
		}
	}

	private static ApiKey? ApiCheck(HttpContext ctx, long deklaracija){
		var api = ctx.GetAPI();
		if(api is not null && api.Deklaracija==deklaracija) return api;
		Error.E403(ctx,true); return null;
	}
}