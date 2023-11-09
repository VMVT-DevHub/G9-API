using System.Text.Json;

namespace App.Roles;


/// <summary>Rolės modelis</summary>
public class Role {
	/// <summary>Identifikacinis numeris</summary>
	public long? ID { get; set; }
	/// <summary>Sisteminis vardas</summary>
	public string? Name { get; set; }
	/// <summary>Pavadinimas</summary>
	public string? Title { get; set; }
	/// <summary>Aprašymas</summary>
	public string? Descr { get; set; }


	/// <summary>Rolės sukūrimo funkcija</summary>
	/// <param name="name">Sisteminis vardas</param>
	/// <param name="title">Pavadinimas</param>
	/// <param name="desc">Aprašymas</param>
	/// <returns>Sukurta rolė</returns>
	public static Role Create(string name, string? title, string? desc){
		if(string.IsNullOrWhiteSpace(name)) return new RoleError(1203,"Nenurodytas rolės pavadinimas",new Role(){ Name=name,Title=title,Descr=desc });
		using var db = new DBExec("SELECT id,msg FROM app.role_add(@name,@title,@desc);",("@name",name),("@title",title),("@desc",desc));
		using var rdr = db.GetReader();
		if(rdr.Read()){
			if(rdr.IsDBNull(0))
				return new RoleError(1202, rdr.GetStringN(1) ?? "Nepavyko sukurti rolės", new Role(){ Name=name,Title=title,Descr=desc });
			return new Role(){ ID=rdr.GetInt64(0), Name=name, Title=title, Descr=desc };
		}
		return new RoleError(1201,"Netikėta klaida kuriant rolę", new Role(){ Name=name,Title=title,Descr=desc });
	}

	/// <summary>Rolės atnaujinimo funkcija</summary>
	/// <param name="role">Rolė</param>
	/// <returns>Atnaujinta rolė</returns>
	public static Role Update(Role role){
		if(role.ID<=0) return new RoleError(1205,"Nenurodytas rolės identifikatorius",role);
		if(string.IsNullOrWhiteSpace(role.Name)) return new RoleError(1206,"Nenurodytas rolės pavadinimas",role);
		var msg = new DBExec("SELECT msg FROM app.role_mod(@id,@name,@title,@desc);",("@id",role.ID),("@name",role.Name),("@title",role.Title),("@desc",role.Descr)).ExecuteScalar()?.ToString();
		if(string.IsNullOrEmpty(msg)) return role;
		return new RoleError(1204,"Nepavyko atnaujinti rolės", role);
	}

	/// <summary>Rolės trynimo funkcija</summary>
	/// <param name="id">Rolės identifikatorius</param>
	/// <returns></returns>
	public static bool Delete(long id){
		var msg = new DBExec("SELECT msg FROM app.role_mod(@id,@name,@title,@desc);",("@id",id)).ExecuteScalar()?.ToString();
		return string.IsNullOrWhiteSpace(msg); //TODO: Užloginti klaidą
	}

	/// <summary>Gauti rolę</summary>
	/// <param name="id">Rolės identifikatorius</param>
	/// <returns>Rolė</returns>
	public static Role? Get(long id){
		using var db = new DBExec("SELECT role_id,role_name,role_title,role_desc FROM app.app_roles WHERE role_id=@id;","@id",id);
		using var rdr = db.GetReader();
		return rdr.Read() ? new Role(){ ID=rdr.GetInt64(0), Name=rdr.GetString(1), Title=rdr.GetStringN(3), Descr=rdr.GetStringN(4) } : null;
	}
}

/// <summary>Rolės klaida</summary>
public class RoleError : Role {
	/// <summary>Klaidos kodas</summary>
	public int Code { get; set; }
	/// <summary>Klaidos žinutė</summary>
	public string Message { get; set; }
	/// <summary>Detalės</summary>
	public string? ErrorData { get; set; }

	/// <summary>Rolės klaidos konstruktorius</summary>
	/// <param name="code"></param><param name="msg"></param><param name="data"></param>
	public RoleError(int code, string msg, string? data){ Code=code; Message=msg; ErrorData=data; }
	
	/// <summary>Rolės klaidos konstruktorius</summary>
	/// <param name="code"></param><param name="msg"></param><param name="data"></param>
	public RoleError(int code, string msg, object? data){ Code=code; Message=msg; ErrorData=JsonSerializer.Serialize(data); }
}


/// <summary>Rolių sąrašas</summary>
public class Roles : List<Role> {
	private Dictionary<long, Role> CachedIndex { get; } = new();
	/// <summary>Sąrašo konstruktorius</summary>
	public Roles() {
		using var db = new DBExec("SELECT role_id,role_name,role_title,role_desc FROM app.app_roles;");
		using var rdr = db.GetReader();
		while(rdr.Read()){
			var rle = new Role(){ ID=rdr.GetInt64(0), Name=rdr.GetString(1), Title=rdr.GetStringN(2), Descr=rdr.GetStringN(3)};
			Add(rle); CachedIndex[rle.ID??0]=rle;
		}
	}

	/// <summary>Gauti rolę pagal identifikatorių</summary>
	/// <param name="id">Rolės Identifikatorius</param>
	/// <param name="role">Rolė</param>
	/// <returns>T/N ar rolė yra</returns>
	public bool TryGet(long id, out Role? role) =>  CachedIndex.TryGetValue(id,out role);

	/// <summary>Gauti rolę pagal identifikatorių</summary>
	/// <param name="id">Rolės identifikatorius</param>
	/// <returns>Rolė</returns>
	public Role? Get(long id) => CachedIndex.TryGetValue(id, out var role) ? role : null;
}

