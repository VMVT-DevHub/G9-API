using System.Text.Json;
using App.Auth;
using G9.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.API;

/// <summary>Deklaravimo API</summary>
public static class Deklaravimas {
	/// <summary>Deklaracijos validacija</summary>
	/// <param name="ctx"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="ct"></param><returns></returns>
	public static async Task Valid(HttpContext ctx, int deklaracija, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)) await PrintDeklar(ctx,deklaracija,ct);
	}

	private static async Task PrintDeklar(HttpContext ctx, int deklaracija, CancellationToken ct, DeklarTipas? tipas=null, Err? error=null){
		ctx.Response.ContentType="application/json";
		var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
		using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
		writer.WriteStartObject();
		var rls = ctx.GetUser()?.Roles?.ToArray();
		var prms = new DBParams(("@deklar", deklaracija),("@usr", ctx.GetUser()?.ID));		
		if(tipas is null || tipas == DeklarTipas.Trukumas){
			writer.WritePropertyName("Trukumas");
			await DBExtensions.PrintArray("SELECT * FROM public.valid_trukumas_set(@deklar,@usr);", prms, writer, ct, null, error?.Trukumas);
		}		
		if(tipas is null || tipas == DeklarTipas.Kartojasi){
			writer.WritePropertyName("Kartojasi");
			await DBExtensions.PrintArray("SELECT * FROM public.valid_kartojasi_set(@deklar,@usr);", prms, writer, ct, null, error?.Kartojasi);
		}
		if(tipas is null || tipas == DeklarTipas.Virsijimas){
			writer.WritePropertyName("Virsijimas");
			await DBExtensions.PrintArray("SELECT * FROM public.valid_virsija_set(@deklar,@usr);", prms, writer, ct, null, error?.Virsijimas);
		}
		writer.WriteEndObject();
		await writer.FlushAsync(ct);
	}

	private class Err {
		public List<Klaida>? Trukumas { get; set; }
		public List<Klaida>? Kartojasi { get; set; }
		public List<Klaida>? Virsijimas { get; set; }
	}

	/// <summary>Deklaracijos pateikimas (NEVEIKIA)</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="data">Neatitikimų patvirtinimo duomenys</param>
	public static async Task Submit(HttpContext ctx, int deklaracija, [FromBody] DeklaravimasSet data, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)){
			if(data is not null) {
				await Save(ctx, deklaracija, data, ct);
			}
		}
	}
	
	/// <summary>Deklaracijos neatitikimų pildymas</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="tipas">Deklaracijos neatitikimų tipas</param>
	/// <param name="data">Neatitikimų patvirtinimo duomenys</param>
	public static async Task UpdateOne(HttpContext ctx, int deklaracija, [FromRoute] DeklarTipas tipas, [FromBody] DeklaravimasSet data, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)) await PrintDeklar(ctx,deklaracija,ct,tipas,await Save(ctx, deklaracija, data, ct));
	}

	/// <summary>Deklaracijos neatitikimų gavimas pagal tipą</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="tipas">Deklaracijos neatitikimų tipas</param>
	public static async Task GetOne(HttpContext ctx, int deklaracija, [FromRoute] DeklarTipas tipas, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)) await PrintDeklar(ctx,deklaracija,ct,tipas);
	}


	private static readonly string SqlUpdateTrukumas = "UPDATE public.valid_trukumas SET vld_tvirtinti=@tvrt,vld_pastabos=@pstb,vld_user=@usr,vld_date_modif=timezone('utc', now()) WHERE vld_id=@id and vld_deklar=@deklar;";
	private static readonly string SqlUpdateKartojasi = "UPDATE public.valid_kartojasi SET vld_tvirtinti=@tvrt,vld_pastabos=@pstb,vld_user=@usr,vld_date_modif=timezone('utc', now()) WHERE vld_id=@id and vld_deklar=@deklar;";
	private static readonly string SqlUpdateVirsija = "UPDATE public.valid_virsija SET vld_tvirtinti=@tvrt,vld_pastabos=@pstb,vld_user=@usr,vld_date_modif=timezone('utc', now()),vld_nereiksm=@nereik,vld_nereiksm_apras=@nereikapras,vld_zmones=@zmones,vld_loq_reiksme=@loqr,vld_loq_verte=@loqv,vld_statusas=@stat,vld_tipas=@tipas WHERE vld_deklar=@deklar and vld_id=@id;";
	private static async Task<Err> Save(HttpContext ctx, int deklaracija, DeklaravimasSet data, CancellationToken ct){
		var usr = ctx.GetUser()?.ID; var err = new Err();
		using var db = new DBExec(""){ UseTransaction=true };
		if(data.Trukumas?.Count>0){
			var param = new DBParams(("@deklar",deklaracija),("@usr",usr),("@id",0),("@tvirt",false),("@past",""));
			foreach(var i in data.Trukumas){
				if(i.Patvirtinta && i.Pastabos?.Length<5) {
					(err.Trukumas??=new()).Add(new(i.ID,"Neįvesta arba per trumpa pastaba"));
				}
				else {
					param.Data["@id"]=i.ID; param.Data["@tvrt"]=i.Patvirtinta; param.Data["@pstb"]=i.Pastabos;
					await db.Set(SqlUpdateTrukumas,param).Execute(ct);
				}
			}
		}
		if(data.Kartojasi?.Count>0){
			var param = new DBParams(("@deklar",deklaracija),("@usr",usr),("@id",0),("@tvirt",false),("@past",""));
			foreach(var i in data.Kartojasi){
				param.Data["@id"]=i.ID; param.Data["@tvrt"]=i.Patvirtinta; param.Data["@pstb"]=i.Pastabos;
				await db.Set(SqlUpdateKartojasi,param).Execute(ct);
			}
		}
		if(data.Virsijimas?.Count>0){
			var param = new DBParams(("@deklar",deklaracija),("@usr",usr),("@id",0),("@tvirt",false),("@past",""));
			foreach(var i in data.Virsijimas){
				//TODO - pass info
				if(i.Patvirtinta) {
					if(i.Nereiksmingas is null) { 
						(err.Virsijimas??=new()).Add(new(i.ID,"Nepažymėtas reikšmingumas")); continue; 
					} else if(i.Nereiksmingas.Value && i.NereiksmApras?.Length<5) { 
						(err.Virsijimas??=new()).Add(new(i.ID,"Neįvestas arba per trumpas nereikšmingo viršijimo pagrindimas")); continue; 
					}
				}
				param.Data["@id"]=i.ID; param.Data["@tvrt"]=i.Patvirtinta; param.Data["@pstb"]=i.Pastabos;
				param.Data["@nereik"]=i.Nereiksmingas;
				param.Data["@nereikapras"]=i.NereiksmApras;
				param.Data["@zmones"]=i.Zmones;
				param.Data["@loqr"]=i.LOQReiksme;
				param.Data["@loqv"]=i.LOQVerte;
				param.Data["@stat"]=i.Statusas;
				param.Data["@tipas"]=i.Tipas;
				await db.Set(SqlUpdateVirsija,param).Execute(ct);				
			}
		}
		db.Transaction?.Commit();
		return err;
	}


	private static async Task<bool> Validate(HttpContext ctx, long deklaracija, CancellationToken ct){
		using var db = new DBExec("SELECT dkl_gvts, dkl_status, dkl_metai, dkl_kiekis FROM deklaravimas WHERE dkl_id=@id;","@id",deklaracija);
		using var rdr = await db.GetReader(ct);
		if(rdr.Read()){
			if(ctx.GetUser()?.Roles?.Contains(rdr.GetInt64(0)) == true){	
				if(rdr.GetDoubleN(3)>0) {	
					var status = rdr.GetInt32(1);
					if(status==3) Error.E422(ctx,true,$"Ši deklaracija jau pateikta.");
					else {					
						if(status!=2 && ctx.Request.Method=="POST") 
							Error.E422(ctx,true,$"Šios deklaracijos pateikti negalima.");
						else return true;
					}
				} else Error.E422(ctx,true,"Neįvestas deklaruojamas vandens kiekis.");
			} else Error.E403(ctx,true);
		} else Error.E404(ctx,true);
		return false;
	}
}
