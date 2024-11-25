using System.Text.Json;
using App.Auth;
using G9.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.API;

/// <summary>Deklaravimo API</summary>
public static class Deklaravimas {	
	/// <summary>Deklaravimo viršijimo statinės reikšmės</summary>
	public static CachedLookup DeklarVirsijimasVal { get; } = new CachedLookup("Virsijimas", ("Tipas","lkp_vietos_tipas"),("Statusas","lkp_stebejimo_statusas"),("Veiksmas","lkp_virs_taisomas_veiksmas"),("Priezastis","lkp_virs_taisomas_priezastis"));

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
			await DBExtensions.PrintArray("SELECT * FROM g9.valid_trukumas_set(@deklar,@usr);", prms, writer, ct, null, error?.Trukumas);
		}		
		if(tipas is null || tipas == NeatitikciuTipas.Kartojasi){
			writer.WritePropertyName("Kartojasi");
			await DBExtensions.PrintArray("SELECT * FROM g9.valid_kartojasi_set(@deklar,@usr);", prms, writer, ct, null, error?.Kartojasi);
		}
		if(tipas is null || tipas == NeatitikciuTipas.Virsijimas){
			writer.WritePropertyName("Virsijimas");
			await DBExtensions.PrintArray("SELECT * FROM g9.valid_virsija_set(@deklar,@usr);", prms, writer, ct, DeklarVirsijimasVal, error?.Virsijimas);
		}
		writer.WriteEndObject();
		await writer.FlushAsync(ct);
	}

	private class Err {
		public List<Klaida>? Trukumas { get; set; }
		public List<Klaida>? Kartojasi { get; set; }
		public List<Klaida>? Virsijimas { get; set; }
	}

	/// <summary>Deklaracijos pateikimas</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="deklaracija">Deklaracijos ID</param>
	public static async Task Submit(HttpContext ctx, int deklaracija, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)){
			var usr = ctx.GetUser()??new();
			var prms = new DBParams(("@deklar",deklaracija),("@usr",usr.ID));			
			await new DBExec("SELECT g9.valid_trukumas_set(@deklar,@usr), g9.valid_kartojasi_set(@deklar,@usr), g9.valid_virsija_set(@deklar,@usr);", prms).Execute(ct);
			using var db = new DBExec("SELECT kartojasi, trukumas, virsija FROM g9.valid_nepatvirtinta(@deklar);",prms);
			using var rdr = await db.GetReader(ct);
			if(await rdr.ReadAsync(ct)){
				var ret = new G9.Models.Deklaravimas(){
					Kartojasi=rdr.GetInt32(0),
					Trukumas=rdr.GetInt32(1),
					Virsijimas=rdr.GetInt32(2),
				};
				
				if(!ret.Klaida){
					var param = new DBParams(("@id",deklaracija),("@name",usr.FullName),("@usr",usr.ID));
					await new DBExec("UPDATE g9.deklaravimas SET dkl_status=3, dkl_deklar_date=timezone('utc',now()), dkl_deklar_user=@name, dkl_deklar_user_id=@usr,"+
						" dkl_modif_date=timezone('utc',now()), dkl_modif_user=@name, dkl_modif_user_id=@usr WHERE dkl_id=@id;", param).Execute(ct);
					ret.Statusas = "Deklaruota";
				} else { ret.Statusas = "Yra nepatvirtintų neatitikčių"; }
				await ctx.Response.WriteAsJsonAsync(ret, ct);
			} else Error.E404(ctx,true);
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

	
	private static readonly string SqlUpdateTrukumas = "UPDATE g9.valid_trukumas SET vld_tvirtinti=@tvrt,vld_kitas=@kitas,vld_pastabos=@pstb,vld_user=@usr,vld_date_modif=timezone('utc', now()) WHERE vld_id=@id and vld_deklar=@deklar;";
	private static readonly string SqlUpdateKartojasi = "UPDATE g9.valid_kartojasi SET vld_tvirtinti=@tvrt,vld_pastabos=@pstb,vld_user=@usr,vld_date_modif=timezone('utc', now()) WHERE vld_id=@id and vld_deklar=@deklar;";
	private static readonly string SqlUpdateVirsija = "UPDATE g9.valid_virsija SET vld_tvirtinti=@tvrt,vld_pastabos=@pstb,vld_user=@usr,vld_date_modif=timezone('utc', now()),vld_nereiksm=@nereik,vld_nereiksm_apras=@nereikapras,vld_zmones=@zmones,vld_loq_reiksme=@loqr,vld_loq_verte=@loqv,vld_statusas=@stat,vld_tipas=@tipas,vld_priez=@tspriez,vld_veiksmas=@tsveiksm,vld_pradzia=@tsprad,vld_pabaiga=@tspab WHERE vld_deklar=@deklar and vld_id=@id;";
	private static async Task<Err> Save(HttpContext ctx, int deklaracija, NeatitiktysSet data, CancellationToken ct){
		var usr = ctx.GetUser()?.ID; var err = new Err();
		using var db = new DBExec(""){ UseTransaction=true };
		if(data.Trukumas?.Count>0){
			var param = new DBParams(("@deklar",deklaracija),("@usr",usr),("@id",0),("@tvirt",false),("@past",""));
			foreach(var i in data.Trukumas){
				if(i.Patvirtinta && i.Pastabos?.Length<5) {
					(err.Trukumas ??= []).Add(new(i.ID, "Neįvesta arba per trumpa pastaba"));
					i.Patvirtinta=false;
				}
				param.Data["@id"]=i.ID; param.Data["@tvrt"]=i.Patvirtinta; param.Data["@pstb"]=i.Pastabos;
				param.Data["@kitas"]=i.KitasDaznumas; await db.Execute(SqlUpdateTrukumas,param,ct);
			}
		}
		if(data.Kartojasi?.Count>0){
			var param = new DBParams(("@deklar",deklaracija),("@usr",usr),("@id",0),("@tvirt",false),("@past",""));
			foreach(var i in data.Kartojasi){
				param.Data["@id"]=i.ID; param.Data["@tvrt"]=i.Patvirtinta; param.Data["@pstb"]=i.Pastabos;
				await db.Execute(SqlUpdateKartojasi,param,ct);
			}
		}
		if(data.Virsijimas?.Count>0){
			DeklarVirsijimasVal.TryGetValue("Tipas",out var tps); tps??=[];
			DeklarVirsijimasVal.TryGetValue("Statusas",out var sts); sts??=[];
			DeklarVirsijimasVal.TryGetValue("Veiksmas",out var vks); vks??=[];
			DeklarVirsijimasVal.TryGetValue("Priezastis",out var prz); prz??=[];
			var param = new DBParams(("@deklar",deklaracija),("@usr",usr),("@id",0),("@tvirt",false),("@past",""));
			foreach(var i in data.Virsijimas){
				if(i.ID>0){
					if(i.Patvirtinta) {
						string? msg=null;
						if(new DBExec("SELECT g9.valid_virsija_detales(@id);","@id",i.ID).ExecuteScalar<bool>()){
							if(i.Nereiksmingas is null) msg="Nepažymėtas reikšmingumas";
							else if(i.Nereiksmingas.Value && !(i.NereiksmApras?.Length>4)) msg="Neįvestas arba per trumpas nereikšmingo viršijimo pagrindimas";
							else if(i.LOQVerte is null) msg="Nepažymėta LOQ vertė";
							else if(i.LOQVerte.Value && (i.LOQReiksme??0)==0) msg="Neįvesta LOQ vertė";
							else if(!(i.Zmones>0)) msg="Neįvestas paveiktų žmonių skaičius";
							else if(!tps.ContainsKey(i.Tipas?.ToString()??"")) msg="Nepasirinktas mėginių ėmimo vietos tipas";
							else if(!sts.ContainsKey(i.Statusas?.ToString()??"")) msg="Nepasirinktas stebėjimo statusas";
						}

						if(msg is null){
							if(!vks.ContainsKey(i.Veiksmas??"")) msg="Nepasirinktas viršijimo taisomasis veiksmas";
							else if(!prz.ContainsKey(i.Priezastis.ToString()??"")) msg="Nepasirinkta viršijimo priežastis";
							else if(i.Pradzia==DateOnly.MinValue) msg="Nepasirinkta taisomojo veiksmo pradžios data";
							else if(i.Pabaiga==DateOnly.MinValue) msg="Nepasirinkta taisomojo veiksmo pabaigos data";
							else if(i.Pabaiga>i.Pabaiga) msg="Taisomojo veiksmo pradžios data negali būti vėlesne negu pabaigos";
						}

						if(!string.IsNullOrEmpty(msg)){ (err.Virsijimas??=[]).Add(new(i.ID,msg)); i.Patvirtinta=false; }
					}
					param.Data["@id"]=i.ID; param.Data["@tvrt"]=i.Patvirtinta; param.Data["@pstb"]=i.Pastabos;
					param.Data["@nereik"]=i.Nereiksmingas; param.Data["@nereikapras"]=i.NereiksmApras; param.Data["@zmones"]=i.Zmones; 
					param.Data["@loqr"]=i.LOQReiksme; param.Data["@loqv"]=i.LOQVerte; param.Data["@stat"]=i.Statusas; param.Data["@tipas"]=i.Tipas;
					param.Data["@tsveiksm"]=i.Veiksmas; param.Data["@tspriez"]=i.Priezastis; param.Data["@tsprad"]=i.Pradzia; param.Data["@tspab"]=i.Pabaiga;
					await db.Execute(SqlUpdateVirsija,param,ct);
				}
			}
		}
		db.Transaction?.Commit();
		return err;
	}


	/// <summary>Tikrinti ar galima deklaruoti</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="deklaracija">Deklaracijos ID</param><param name="skipkiek">Praleisti kiekio validaciją</param>
	public static async Task<bool> Validate(HttpContext ctx, long deklaracija, CancellationToken ct, bool skipkiek=false){
		using var db = new DBExec("SELECT dkl_gvts, dkl_status, dkl_metai, dkl_kiekis FROM g9.deklaravimas WHERE dkl_id=@id;","@id",deklaracija);
		using var rdr = await db.GetReader(ct);
		if(rdr.Read()){
			if(ctx.GetUser()?.Roles?.Contains(rdr.GetInt64(0)) == true){	
				if(skipkiek || rdr.GetDoubleN(3)>0) {	
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
