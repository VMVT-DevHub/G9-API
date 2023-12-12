using System.Text.Json;
using App.Auth;
using G9.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.API;

/// <summary>Deklaravimo API</summary>
public static class Deklaravimas {	
	/// <summary>Deklaravimo viršijimo statinės reikšmės</summary>
	public static CachedLookup DeklarVirsijimasVal { get; } = new CachedLookup("Virsijimas", ("Tipas","lkp_vietos_tipas"),("Statusas","lkp_stebejimo_statusas"));

	/// <summary>Deklaracijos validacija</summary>
	/// <param name="ctx"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="ct"></param><returns></returns>
	public static async Task Valid(HttpContext ctx, int deklaracija, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)) await PrintDeklar(ctx,deklaracija,ct);
	}

	private static async Task PrintDeklar(HttpContext ctx, int deklaracija, CancellationToken ct, NeatitikciuTipas? tipas=null, Err? error=null){
		ctx.Response.ContentType="application/json";
		var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
		using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
		writer.WriteStartObject();
		
		var prms = new DBParams(("@deklar", deklaracija),("@usr", ctx.GetUser()?.ID));
		if(tipas is null || tipas == NeatitikciuTipas.Trukumas){
			writer.WritePropertyName("Trukumas");
			await DBExtensions.PrintArray("SELECT * FROM public.valid_trukumas_set(@deklar,@usr);", prms, writer, ct, null, error?.Trukumas);
		}		
		if(tipas is null || tipas == NeatitikciuTipas.Kartojasi){
			writer.WritePropertyName("Kartojasi");
			await DBExtensions.PrintArray("SELECT * FROM public.valid_kartojasi_set(@deklar,@usr);", prms, writer, ct, null, error?.Kartojasi);
		}
		if(tipas is null || tipas == NeatitikciuTipas.Virsijimas){
			writer.WritePropertyName("Virsijimas");
			await DBExtensions.PrintArray("SELECT * FROM public.valid_virsija_set(@deklar,@usr);", prms, writer, ct, DeklarVirsijimasVal, error?.Virsijimas);
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
	public static async Task Submit(HttpContext ctx, int deklaracija, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)){
			await ctx.Response.WriteAsync(JsonSerializer.Serialize( new G9.Models.Deklaravimas()),ct);
		}
	}
	
	/// <summary>Deklaracijos neatitikčių patvirtinimas</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="data">neatitikčių patvirtinimo duomenys</param>
	public static async Task Update(HttpContext ctx, int deklaracija, [FromBody] NeatitiktysSet data, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)) await PrintDeklar(ctx,deklaracija,ct,null,await Save(ctx, deklaracija, data, ct));
	}
	
	/// <summary>Deklaracijos neatitikčių pildymas</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="tipas">Deklaracijos neatitikčių tipas</param>
	/// <param name="data">Neatitiktčių patvirtinimo duomenys</param>
	public static async Task UpdateOne(HttpContext ctx, int deklaracija, [FromRoute] NeatitikciuTipas tipas, [FromBody] NeatitiktysSet data, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)) await PrintDeklar(ctx,deklaracija,ct,tipas,await Save(ctx, deklaracija, data, ct));
	}

	/// <summary>Deklaracijos neatitikčių gavimas pagal tipą</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="tipas">Deklaracijos neatitikčių tipas</param>
	public static async Task GetOne(HttpContext ctx, int deklaracija, [FromRoute] NeatitikciuTipas tipas, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)) await PrintDeklar(ctx,deklaracija,ct,tipas);
	}


	private static readonly string SqlUpdateTrukumas = "UPDATE public.valid_trukumas SET vld_tvirtinti=@tvrt,vld_kitas=@kitas,vld_pastabos=@pstb,vld_user=@usr,vld_date_modif=timezone('utc', now()) WHERE vld_id=@id and vld_deklar=@deklar;";
	private static readonly string SqlUpdateKartojasi = "UPDATE public.valid_kartojasi SET vld_tvirtinti=@tvrt,vld_pastabos=@pstb,vld_user=@usr,vld_date_modif=timezone('utc', now()) WHERE vld_id=@id and vld_deklar=@deklar;";
	private static readonly string SqlUpdateVirsija = "UPDATE public.valid_virsija SET vld_tvirtinti=@tvrt,vld_pastabos=@pstb,vld_user=@usr,vld_date_modif=timezone('utc', now()),vld_nereiksm=@nereik,vld_nereiksm_apras=@nereikapras,vld_zmones=@zmones,vld_loq_reiksme=@loqr,vld_loq_verte=@loqv,vld_statusas=@stat,vld_tipas=@tipas WHERE vld_deklar=@deklar and vld_id=@id;";
	private static async Task<Err> Save(HttpContext ctx, int deklaracija, NeatitiktysSet data, CancellationToken ct){
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
					param.Data["@kitas"]=i.KitasDaznumas; await db.Set(SqlUpdateTrukumas,param).Execute(ct);
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
			DeklarVirsijimasVal.TryGetValue("Tipas",out var tps); tps??=new();
			DeklarVirsijimasVal.TryGetValue("Statusas",out var sts); sts??=new();
			var param = new DBParams(("@deklar",deklaracija),("@usr",usr),("@id",0),("@tvirt",false),("@past",""));
			foreach(var i in data.Virsijimas){
				if(i.Patvirtinta) {
					string? msg=null;
					if(i.Nereiksmingas is null) msg="Nepažymėtas reikšmingumas";
					else if(i.Nereiksmingas.Value && !(i.NereiksmApras?.Length>4)) msg="Neįvestas arba per trumpas nereikšmingo viršijimo pagrindimas";
					else if(i.LOQVerte is null) msg="Nepažymėta LOQ vertė";
					else if(i.LOQVerte.Value && (i.LOQReiksme??0)==0) msg="Neįvesta LOQ vertė";
					else if(!(i.Zmones>0)) msg="Neįvestas paveiktų žmoniu skaičius";
					else if(tps.ContainsKey(i.Tipas?.ToString()??"")) msg="Nepasitinktas mėginių ėmimo vietos tipas";
					else if(sts.ContainsKey(i.Statusas?.ToString()??"")) msg="Nepasirinktas stebėjimo statusas";
					if(!string.IsNullOrEmpty(msg)){ (err.Virsijimas??=new()).Add(new(i.ID,msg)); i.Patvirtinta=false; }
				}
				param.Data["@id"]=i.ID; param.Data["@tvrt"]=i.Patvirtinta; param.Data["@pstb"]=i.Pastabos;
				param.Data["@nereik"]=i.Nereiksmingas; param.Data["@nereikapras"]=i.NereiksmApras; param.Data["@zmones"]=i.Zmones; 
				param.Data["@loqr"]=i.LOQReiksme; param.Data["@loqv"]=i.LOQVerte; param.Data["@stat"]=i.Statusas; param.Data["@tipas"]=i.Tipas;
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
