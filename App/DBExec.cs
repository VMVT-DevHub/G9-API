
using System.Text.Json;
using Npgsql;


namespace App;

/// <summary>Duomenų bazės konfiguracija</summary>
public static class DBProps {

	/// <summary>Duombazės prisijungimas</summary>
	public static string ConnString { get; set; } = Startup.ConnStr;
}

/// <summary>Postgres konektorius</summary>
public class DBExec : IDisposable {
	/// <summary>Duomenų bazės užklausų atvaizdavimas konsolėje</summary>
	public static bool Debug { get; set; } = Config.GetBool("Config","DebugDB",false);
	/// <summary>Duomenų bazės užklausa</summary>
	public string SQL { get; set; }
	/// <summary>Užklausos parametrai</summary>
	public DBParams Params { get; set; }

	private NpgsqlConnection? Conn { get; set; }
	private NpgsqlCommand? Cmd { get; set; }

	private NpgsqlCommand Command(string sql){
		Conn ??= new NpgsqlConnection(DBProps.ConnString); Conn.Open();
		Cmd ??= new NpgsqlCommand(sql,Conn); Params?.Load(Cmd);
		return Cmd;
	}
	private async Task<NpgsqlCommand> CommandAsync(string sql, CancellationToken ct){
		Conn ??= new NpgsqlConnection(DBProps.ConnString); await Conn.OpenAsync(ct);
		Cmd ??= new NpgsqlCommand(sql,Conn); Params?.Load(Cmd);
		return Cmd;
	}

	/// <summary>Duomenų bazės prisijungimo sukurimas</summary>
	/// <param name="sql">Užklausa</param>
	public DBExec(string sql){ SQL=sql; Params = new(); }

	/// <summary>Duomenų bazės prisijungimo sukurimas</summary>
	/// <param name="sql">Užklausa</param>
	/// <param name="param">Parametras</param>
	/// <param name="value">Reikšmė</param>
	public DBExec(string sql, string param, object? value){ SQL=sql; Params = new(); Params.Add(param,value); }

	/// <summary>Duomenų bazės prisijungimo sukurimas</summary>
	/// <param name="sql">Užklausa</param>
	/// <param name="param">Parametrai</param>
	public DBExec(string sql, params ValueTuple<string, object?>[] param){ SQL=sql; Params = new(param); }

	/// <summary>Duomenų bazės prisijungimo sukurimas</summary>
	/// <param name="sql">Užklausa</param>
	/// <param name="param">Parametrai</param>
	public DBExec(string sql, DBParams param){ SQL=sql; Params=param; }

	/// <summary>Gauti duomenų skaitytuvą</summary>
	/// <returns>NpgsqlDataReader</returns>
	public NpgsqlDataReader GetReader() => Command(SQL).ExecuteReader();

	/// <summary>Gauti duomenų skaitytuvą</summary>
	/// <returns>NpgsqlDataReader</returns>
	public async Task<NpgsqlDataReader> GetReaderAsync(CancellationToken ct) => await (await CommandAsync(SQL,ct)).ExecuteReaderAsync(ct);


	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public int Execute() { var ret = Command(SQL).ExecuteNonQuery(); Dispose(); return ret; }

	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public async Task<int> Execute(CancellationToken ct) { var ret = await(await CommandAsync(SQL,ct)).ExecuteNonQueryAsync(ct); Dispose(); return ret;}
	
	
	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public object? ExecuteScalar() { var ret = Command(SQL).ExecuteScalar(); Dispose(); return ret;}

	/// <summary>Vykdyti SQL užklausą grąžinant reikšmę</summary>
	/// <returns>Įrašų skaičius</returns>
	public T? ExecuteScalar<T>() { var ret = Command(SQL).ExecuteScalar(); Dispose(); return ret is T t ? t: default;}


	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public async Task<object?> ExecuteScalar(CancellationToken ct) { var ret = await(await CommandAsync(SQL,ct)).ExecuteScalarAsync(ct); Dispose(); return ret; }

	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public async Task<T?> ExecuteScalar<T>(CancellationToken ct) { var ret = await(await CommandAsync(SQL,ct)).ExecuteScalarAsync(ct); Dispose(); return ret is T t ? t: default; }


	// To detect redundant calls
	private bool IsDisposed;
	/// <summary>Duomenų bazės uždarymo metodas</summary>
	public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
	/// <summary>Duomenų bazės uždarymo metodas</summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing) {
		if (!IsDisposed) {
			if (disposing) {
				Cmd?.Dispose(); Cmd=null;
				Conn?.Dispose(); Conn=null;
				//Dispose stuff;
			}
			IsDisposed = true;
		}
	}

}

/// <summary>PostgreSQL funkcijų išplėtimai</summary>
public static class DBExtensions {
	
	/// <summary></summary><param name="rdr"></param><param name="id"></param><returns></returns>
	public static string? GetStringN(this NpgsqlDataReader rdr,int id) => rdr.IsDBNull(id)?null:rdr.GetString(id);
	/// <summary></summary><param name="rdr"></param><param name="id"></param><returns></returns>
	public static long? GetLongN(this NpgsqlDataReader rdr,int id) => rdr.IsDBNull(id)?null:rdr.GetInt64(id);
	/// <summary></summary><param name="rdr"></param><param name="id"></param><returns></returns>
	public static int? GetIntN(this NpgsqlDataReader rdr,int id) => rdr.IsDBNull(id)?null:rdr.GetInt32(id);
	/// <summary></summary><param name="rdr"></param><param name="id"></param><returns></returns>
	public static DateTime? GetDateTimeN(this NpgsqlDataReader rdr,int id) => rdr.IsDBNull(id)?null:rdr.GetDateTime(id);
	/// <summary></summary><param name="rdr"></param><param name="id"></param><returns></returns>
	public static double? GetDoubleN(this NpgsqlDataReader rdr,int id) => rdr.IsDBNull(id)?null:rdr.GetDouble(id);
	/// <summary></summary><param name="rdr"></param><param name="id"></param><returns></returns>
	public static T? GetObject<T>(this NpgsqlDataReader rdr,int id) => rdr.IsDBNull(id)?default:JsonSerializer.Deserialize<T>(rdr.GetString(id));



	/// <summary>Atiduoti užklausą kaip masyvą į API</summary>
	/// <param name="query">DB užklausa</param>
	/// <param name="dbparams">Užklausos parametrai</param>
	/// <param name="wrt">Json writer</param>
	/// <param name="ct"></param>
	/// <param name="flush">atiduodamas įrašų kiekis</param>
	/// <returns></returns>
	public static async Task PrintArray(string query, DBParams? dbparams,Utf8JsonWriter wrt, CancellationToken ct,int flush=20){
		using var rdr = await new DBExec(query, dbparams??new()).GetReaderAsync(ct);
		wrt.WriteStartArray();
		await Loop(rdr,wrt,ct,flush);
		wrt.WriteEndArray();
	}


	/// <summary>Atiduoti masyvo duomenis į API</summary>
	/// <param name="rdr"></param><param name="wrt"></param><param name="ct"></param>
	/// <param name="flush">atiduodamas įrašų kiekis</param>
	/// <returns></returns>
	public static async Task Loop(NpgsqlDataReader rdr, Utf8JsonWriter wrt, CancellationToken ct, int flush=20){
		var act= new List<Action<int>>();
		var fct = rdr.FieldCount;
		for(var i =0; i<fct; i++) act.Add(GetAct(rdr,wrt,i));
		var cnt=flush;
		while (await rdr.ReadAsync(ct)) {
			wrt.WriteStartObject();
			for(var i=0; i<fct ;i++) act[i](i);
			wrt.WriteEndObject();
			if(cnt--<1){ wrt.Flush(); cnt=flush; }
		}
	}
	
	/// <summary>Objekto duomenis į API</summary>
	/// <param name="rdr"></param><param name="wrt"></param><param name="ct"></param>
	/// <returns></returns>
	public static async Task GetObject(NpgsqlDataReader rdr, Utf8JsonWriter wrt, CancellationToken ct){
		var act= new List<Action<int>>();
		var fct = rdr.FieldCount;
		for(var i =0; i<fct; i++) act.Add(GetAct(rdr,wrt,i));
		if (await rdr.ReadAsync(ct)) for(var i=0; i<fct ;i++) act[i](i);
	}


	/// <summary>Duomenų konvertavimo funkcija</summary>
	/// <param name="rdr">Data reader</param>
	/// <param name="wrt">JSON writer</param>
	/// <param name="i">Duomenų lauko ID</param>
	/// <returns>Funkcija</returns>
	private static Action<int> GetAct(NpgsqlDataReader rdr, Utf8JsonWriter wrt, int i) {
		var tp= rdr.GetFieldType(i); var nm= rdr.GetName(i);
		if(tp==typeof(bool)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteBoolean(nm,rdr.GetBoolean(i));};
		else if(tp==typeof(byte)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetByte(i));};
		else if(tp==typeof(char)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetChar(i));};
		else if(tp==typeof(DateTime)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteString(nm,rdr.GetDateTime(i));};
		else if(tp==typeof(decimal)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetDecimal(i));};
		else if(tp==typeof(double)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetDouble(i));};
		else if(tp==typeof(float)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetFloat(i));};
		else if(tp==typeof(Guid)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteString(nm,rdr.GetGuid(i));};
		else if(tp==typeof(short)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetInt16(i));};
		else if(tp==typeof(int)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetInt32(i));};
		else if(tp==typeof(long)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetInt64(i));};
		else if(tp==typeof(string)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteString(nm,rdr.GetString(i));};
		else if(tp==typeof(TimeSpan)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetTimeSpan(i).Ticks);};
		else return (i)=>{};
	} 
}

/// <summary>Duomenų bazės užklausos parametrai</summary>
public class DBParams {
	/// <summary>Parametrai</summary>
	public Dictionary<string, object?> Data { get; set; }
	/// <summary>Įkelti parametrus į užklausą</summary>
	/// <param name="cmd">Duomenų bazės komanda</param>
	/// <param name="print">Spausdinti konsolėje</param>
	public void Load(NpgsqlCommand cmd, bool print = false) { foreach (var i in Data) { if (!cmd.Parameters.Contains(i.Key)) { cmd.Parameters.AddWithValue(i.Key, i.Value ?? DBNull.Value); } }
		if (DBExec.Debug) { Console.WriteLine($"[SQL] {cmd.CommandText} {(Data?.Count > 0 ? JsonSerializer.Serialize(Data) : "")}"); }
	}
	/// <summary>Parametrų objekto iniciavimas</summary>
	public DBParams() { Data = new (); }
	/// <summary>Parametrų objekto iniciavimas</summary>
	/// <param name="pairs">Duomenų masyvas</param>
	public DBParams(params ValueTuple<string, object?>[] pairs) { Data = pairs.ToDictionary(x => x.Item1, x => x.Item2); }
	/// <summary>Pridėti parametrą</summary>
	/// <param name="key">Parametro raktas</param>
	/// <param name="val">Parametro reikšmė</param>
	public void Add(string key, object? val) { Data[key] = val; }
}