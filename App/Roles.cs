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
		using var db = new DBExec("SELECT role_id,role_name,role_title,role_desc FROM app.roles;");
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

