using System.Text;
using System.Text.Json;
using App.Roles;

namespace App.Users;


/// <summary>Vartotojo informacija</summary>
public class User {
	/// <summary>Sisteminis vartotojo id</summary>
	public Guid? ID { get; set; }
	/// <summary>Asmens Kodas</summary>
	public long? AK { get; set; }
	/// <summary>Vardas</summary>
	public string? FName { get; set; }
	/// <summary>Pavardė</summary>
	public string? LName { get; set; }
	/// <summary>El.Paštas</summary>
	public string? Email { get; set; }
	/// <summary>Telefono numeris</summary>
	public string? Phone { get; set; }
	/// <summary>Prisijungusio vartotojo tipas</summary>
	public string? Type { get; set; } = "USER";
	/// <summary>Vartotojo rolės</summary>
	public List<long> Roles { get; set; }

	/// <summary>Vartotojo klaidų tikrinimas ir reportavimas</summary>
	/// <param name="ctx"></param>
	/// <param name="report"></param>
	/// <returns></returns>
	public bool Valid(HttpContext ctx, bool report=true){ if(this is UserError err) { if(report) err.Report(ctx); return false; } return true; }

	/// <summary>Gauti vartotojo roles</summary>
	/// <returns></returns>
	public User GetRoles(){
		using var db = new DBExec("SELECT usrl_role FROM app.app_user_roles WHERE usrl_user=@usr","@usr",ID);
		using var rdr = db.GetReader(); Roles = new(); while(rdr.Read()) Roles.Add(rdr.GetInt64(0)); return this;
	}

	/// <summary>Registruoti prisijungusį vartotoją</summary>
	/// <param name="ak">Asmens Kodas</param>
	/// <param name="fname">Vardas</param>
	/// <param name="lname">Pavardė</param>
	/// <param name="email">El.Paštas</param>
	/// <param name="phone">Telefonas</param>
	/// <param name="ctx"></param>
	/// <returns>Prisijungęs vartotojas</returns>
	public static User Login(long ak, string fname, string lname, string? email, string? phone, HttpContext ctx)
		=> Login(new User(){ AK=ak, FName=fname, LName=lname, Email=email, Phone=phone },ctx);

	private static User Login(User usr, HttpContext ctx, bool create=true){
		using var db = new DBExec("SELECT id,ak,fname,lname,email,phone FROM app.user_login(@ak,@ip,@ua)",("@ak",usr.AK),("@ip",ctx.GetIP()),("@ua",ctx.GetUA()));
		using var rdr = db.GetReader();
		if(rdr.Read()){
			if(!(rdr.IsDBNull(0) || rdr.IsDBNull(1) || rdr.IsDBNull(2) || rdr.IsDBNull(3))) {
				var usd = new User(){ ID=rdr.GetGuid(0), AK=rdr.GetInt64(1), FName=rdr.GetString(2), LName=rdr.GetString(3), Email=rdr.GetStringN(4), Phone=rdr.GetStringN(5) };
				if(usr.FName!=usd.FName || usr.LName!=usd.LName || usr.Email!=usd.Email || usr.Phone!=usd.Phone)
					if(Update(usr) is UserError upd) return upd;
				return usr.GetRoles();
			}
			return new UserError(1102, "Vartotojo informacijos klaida", usr);
		}
		return create ? Login(Create(usr),ctx,true) : new UserError(1101,"Vartotojas nerastas",usr);
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
		var msg = new DBExec("SELECT msg FROM app.user_mod(@ak,@fname,@lname,@email,@phone);", ("@ak",usr.AK),("@fname",usr.FName),("@lname",usr.LName),("@email",usr.Email),("@phone",usr.Phone)).ExecuteScalar()?.ToString();
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
		ctx.Response.Redirect($"/klaida?id={Code}&msg={Convert.ToBase64String(Encoding.UTF8.GetBytes(Message))}");
		return this;
	}
}
