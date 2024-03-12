using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using G9.Models;

namespace App.Users;


/// <summary>Vartotojo informacija</summary>
public class User {
	/// <summary>Sisteminis vartotojo id</summary>
	public Guid? ID { get; set; }
	/// <summary>Asmens Kodas</summary>
	[JsonIgnore] public long? AK { get; set; }
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
		using var db = new DBExec("SELECT role_name FROM app.user_roles LEFT JOIN app.roles on (usrl_role=role_id) WHERE usrl_user=@usr and role_name is not null","@usr",ID);
		using var rdr = db.GetReader(); Roles = new(); Admin = new();

		if(JA is not null){
			var lst = new DBExec("SELECT vkl_id FROM g9.gvts WHERE vkl_ja=@ja","@ja",JA.ID).GetList<long>();
			foreach(var i in lst){ Roles.Add(i); Admin.Add(i); }
		}
		else {
			while(rdr.Read()) {
				var rle = rdr.GetString(0).Split(".");
				if(rle.Length>=2 && long.TryParse(rle[1],out var role)){
					Roles.Add(role); if(rle.Length==3 && rle[2]=="admin") Admin.Add(role);
				}
			}
		}
		return this;
		//Getting role as number
		//using var db = new DBExec("SELECT usrl_role FROM app.app_user_roles WHERE usrl_user=@usr","@usr",ID);
		//using var rdr = db.GetReader(); Roles = new(); while(rdr.Read()) Roles.Add(rdr.GetInt64(0)); return this;
	}

	/// <summary>Registruoti prisijungusį vartotoją</summary>
	/// <param name="ak">Asmens Kodas</param>
	/// <param name="fname">Vardas</param>
	/// <param name="lname">Pavardė</param>
	/// <param name="email">El.Paštas</param>
	/// <param name="phone">Telefonas</param>
	/// <param name="ja">Juridinio asmens kodas</param>
	/// <param name="ctx"></param>
	/// <returns>Prisijungęs vartotojas</returns>
	public static User Login(long ak, string fname, string lname, string? email, string? phone, string? ja, HttpContext ctx)
		=> Login(new User(){ AK=ak, FName=fname, LName=lname, Email=email, Phone=phone}.GetJA(ja) ,ctx);

	private static User Login(User usr, HttpContext ctx, bool create=true){
		using var db = new DBExec("SELECT id,fname,lname,email,phone FROM app.user_login(@ak,@ip,@ua)",("@ak",(object?)usr.ID ?? usr.AK),("@ip",ctx.GetIP()),("@ua",ctx.GetUA()));
		using var rdr = db.GetReader();
		if(rdr.Read()){
			if(!(rdr.IsDBNull(0) || rdr.IsDBNull(1) || rdr.IsDBNull(2) || rdr.IsDBNull(3))) {
				var usd = new User(){ ID=rdr.GetGuid(0), FName=rdr.GetString(1), LName=rdr.GetString(2), Email=rdr.GetStringN(3), Phone=rdr.GetStringN(4) };
				usr.ID=usd.ID; usr.AK=null;
				var ja = usr.JA is not null;
				if(usr.FName!=usd.FName || usr.LName!=usd.LName || 
					//JA - neatnaujinam
					(!ja && (usr.Email!=usd.Email || usr.Phone!=usd.Phone)))
						if(Update(usr) is UserError upd) return upd;
				if(ja) { usr.Email=usd.Email; usr.Phone=usd.Phone; }
				return usr.GetRoles();
			}
			return new UserError(1102, "Vartotojo informacijos klaida", usr);
		}
		return create ? Login(Create(usr),ctx,false) : new UserError(1101,"Vartotojas nerastas",usr);
	}

	private User GetJA(string? id){
		if(long.TryParse(id, out var ja) && ja>0){
			using var db = new DBExec("SELECT ja_title,ja_adresas FROM jar.data where ja_id=@ja;","@ja",ja);
			using var rdr = db.GetReader();
			if(rdr.HasRows && rdr.Read())
				JA = new(){ ID=ja, Title=rdr.GetStringN(0), Addr=rdr.GetStringN(1)};
			else return new UserError(1117,"Juridinia asmuo nerastas registre",this);
		}
		return this;
	}
	private static User Create(User usr){
		using var db = new DBExec("SELECT id,msg FROM app.user_add(@ak,@fname,@lname,@email,@phone);", ("@ak",usr.AK),("@fname",usr.FName),("@lname",usr.LName),("@email",usr.Email),("@phone",usr.Phone));
		using var rdr = db.GetReader();
		if(rdr.Read()){
			if(rdr.IsDBNull(1)){ usr.ID = rdr.GetGuid(0); return usr; }
			return new UserError(1104,rdr.GetString(1),usr);
		} 
		return new UserError(1103,"Vartotojo pridėjimo klaida",usr); 
	}

	private static User Update(User usr){
		var msg = new DBExec("SELECT msg FROM app.user_mod(@ak,@fname,@lname,@email,@phone);", ("@ak",usr.ID),("@fname",usr.FName),("@lname",usr.LName),("@email",usr.Email),("@phone",usr.Phone)).ExecuteScalar()?.ToString();
		return string.IsNullOrEmpty(msg) ? usr : new UserError(1105,msg,usr); 
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
