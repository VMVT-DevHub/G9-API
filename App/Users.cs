using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using G9.Models;

namespace App.Users;


/// <summary>Vartotojo informacija</summary>
public class User {
	/// <summary>Sisteminis vartotojo id</summary>
	public Guid? ID { get; set; }
	/// <summary>Vardas</summary>
	public string? FName { get; set; }
	/// <summary>Pavardė</summary>
	public string? LName { get; set; }
	/// <summary>Asmens Kodas</summary>
	[JsonIgnore] public string FullName => $"{FName} {LName}";

	/// <summary>El.Paštas</summary>
	public string? Email { get; set; }
	/// <summary>Telefono numeris</summary>
	public string? Phone { get; set; }
	/// <summary>Sesijos pratęsimo laikas</summary>
	public DateTime SessionExtend { get; set; }
	/// <summary>Sesijos pabaigos laikas</summary>
	public DateTime SessionExpire { get; set; }
	/// <summary>Juridinio asmens kodas</summary>
	public G9.Models.JA? JA { get; set; }
	/// <summary>Prisijungusio vartotojo tipas</summary>
	public HashSet<long>? Roles { get; set; }
	/// <summary>Vartotojo rolės</summary>
	public HashSet<long>? Admin { get; set; }

	/// <summary>Vartotojo klaidų tikrinimas ir reportavimas</summary>
	/// <param name="ctx"></param>
	/// <param name="report"></param>
	/// <returns></returns>
	public bool Valid(HttpContext ctx, bool report=true){ if(this is UserError err) { if(report) err.Report(ctx); return false; } return true; }

	/// <summary>Gauti vartotojo roles</summary>
	/// <returns></returns>
	public User GetRoles(){
		using var db = new DBExec("SELECT role_gvts, role_admin FROM app.roles WHERE role_user=@usr","@usr",ID);
		using var rdr = db.GetReader(); Roles = []; Admin = [];

		if(JA is not null){
			var lst = new DBExec("SELECT vkl_id FROM g9.gvts WHERE vkl_ja=@ja","@ja",JA.ID).GetList<long>();
			foreach(var i in lst){ Roles.Add(i); Admin.Add(i); }
		}
		else {
			while(rdr.Read()) {
				var rle = rdr.GetInt64(0); Roles.Add(rle); if(rdr.GetBoolean(1)) Admin.Add(rle);
			}
		}
		return this;
	}

	/// <summary>Registruoti prisijungusį vartotoją</summary>
	/// <param name="id">vartotojo id</param>
	/// <param name="fname">Vardas</param>
	/// <param name="lname">Pavardė</param>
	/// <param name="email">El.Paštas</param>
	/// <param name="phone">Telefonas</param>
	/// <param name="ja">Juridinio asmens kodas</param>
	/// <param name="ctx"></param>
	/// <returns>Prisijungęs vartotojas</returns>
	public static User Login(Guid id, string fname, string lname, string? email, string? phone, string? ja, HttpContext ctx)
		=> Login(new User(){ ID=id, FName=fname, LName=lname, Email=email, Phone=phone}.GetJA(ja) ,ctx);

	private static User Login(User usr, HttpContext ctx){
		new DBExec("INSERT INTO app.log_login (log_user,log_ip,log_ua,log_data) VALUES (@id,@ip,@ua,@data::jsonb);",
			("@id",usr.ID),("@ip",ctx.GetIP()),("@ua",ctx.GetUA()),("@data",JsonSerializer.Serialize(usr))).Execute();
		return usr.GetRoles();
	}

	private User GetJA(string? id){
		if(long.TryParse(id, out var ja) && ja>0){
			using var db = new DBExec("SELECT ja_pavadinimas,adresas FROM jar.data where ja_kodas=@ja;","@ja",ja);
			using var rdr = db.GetReader();
			if(rdr.HasRows && rdr.Read())
				JA = new(){ ID=ja, Title=rdr.GetStringN(0), Addr=rdr.GetStringN(1)};
			else return new UserError(1117,"Juridinis asmuo nerastas registre",this);
		}
		return this;
	}
}

/// <summary>Vartotojo prisijungimo klaidos išplėtimas</summary>
public class UserError : User {
	/// <summary>Klaidos kodas</summary>
	public int Code { get; set; }
	/// <summary>Klaidos žnutė</summary>
	public string Message { get; set; }
	/// <summary>Klaidos duomenys</summary>
	public string? ErrorData { get; set; }

	/// <summary>Vartotojo prisijungimo klaidos konstruktorius</summary>
	/// <param name="code"></param>
	/// <param name="msg"></param>
	/// <param name="data"></param>
	public UserError(int code, string msg, string? data){ Code=code; Message=msg; ErrorData=data; }
	
	/// <summary>Vartotojo prisijungimo klaidos konstruktorius</summary>
	/// <param name="code"></param>
	/// <param name="msg"></param>
	/// <param name="data"></param>
	public UserError(int code, string msg, object? data){ Code=code; Message=msg; ErrorData=JsonSerializer.Serialize(data); }

	/// <summary>Reportuoti klaidą</summary>
	/// <param name="ctx"></param>
	/// <returns></returns>
	public UserError Report(HttpContext ctx){
		new DBExec("INSERT INTO app.log_error (log_code,log_msg,log_data,log_ip) VALUES (@code,@msg,@data,@ip);",
			("@code",Code),("@msg",Message),("@data",ErrorData),("@ip",ctx.GetIP())).Execute();
		ctx.Response.Redirect($"{Config.GetVal("Web","Path","/")}klaida?id={Code}&msg={Convert.ToBase64String(Encoding.UTF8.GetBytes(Message))}");
		return this;
	}
}
