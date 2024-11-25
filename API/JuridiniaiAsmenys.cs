using App.Auth;
using App.Users;
using G9.Models;
using System.Text.Json;

namespace App.API;

/// <summary>Juridinių asmenų API</summary>
public static class JuridiniaiAsmenys {
	private async static Task<JADetails?> GetJA(long ja, CancellationToken ct) {
		using var db = new DBExec("SELECT DISTINCT \"ID\",\"Title\",\"Addr\",\"KontaktasVardas\",\"KontaktasPavarde\",\"KontaktasEmail\",\"KontaktasPhone\" FROM g9.v_ja_detales WHERE \"ID\"=@id;", "@id", ja);
		return (await db.GetListAsync<JADetails>(ct)).FirstOrDefault();
	}

	/// <summary>Gauti vartotojo JA detales</summary>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx, CancellationToken ct) {
		ctx.Response.ContentType = "application/json";
		var usr = ctx.GetUser();
		if (usr?.JA?.ID is not null) {
			var ret = await GetJA(usr.JA.ID, ct);
			if (ret?.ID > 0) await ctx.Response.WriteAsJsonAsync(ret, ct);
			else Error.E404(ctx, true);
		} else Error.E403(ctx, true);
	}

	/// <summary>Gauti vartotojo JA detales</summary>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	/// <param name="jad">Juridinio asmens detalės</param>
	/// <returns></returns>
	public static async Task Set(HttpContext ctx, JADetailsSet jad, CancellationToken ct) {
		ctx.Response.ContentType = "application/json";
		var usr = ctx.GetUser();
		if (usr?.JA?.ID is not null) {
			using var db = new DBExec("INSERT INTO your_table (ja_id,jad_kontaktas_vardas,jad_kontaktas_pavarde,jad_kontaktas_email,jad_kontaktas_phone,jad_user) VALUES (@id,@konvardas,@konpavarde,@konemail,@konphone,@usr) " +
				"ON CONFLICT (ja_id) DO UPDATE SET jad_kontaktas_vardas=@konvardas,jad_kontaktas_pavarde=@konpavarde,jad_kontaktas_email=@konemail,jad_kontaktas_phone=@konphone,jad_user=@usr,jad_date=timezone('utc',now());",
				("@id", usr.JA.ID), ("@konvardas", jad.KontaktaiVardas), ("@konpavarde", jad.KontaktaiPavarde), ("@konemail", jad.KontaktaiEmail), ("@konphone", jad.KontaktaiPhone), ("@usr", usr.ID));
			var res = await db.Execute(ct);
			if (res > 0) {
				var ret = await GetJA(usr.JA.ID, ct);
				if (ret?.ID > 0) await ctx.Response.WriteAsJsonAsync(ret, ct);
				else Error.E404(ctx, true);
			}
			else Error.E400(ctx, true, "Pakeitimai nebuvo užsaugoti");
		}
		else Error.E403(ctx, true);
	}
}
