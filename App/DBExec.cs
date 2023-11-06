using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Npgsql;
using Npgsql.Replication;


namespace App;

public class DBExec : IDisposable {
	private static string ConnString { get; set; } = "User ID=postgres;Password=postgres;Server=localhost;Port=5002;Database=postgres;Integrated Security=true;Pooling=true;";
	/// <summary>Duomenų bazės užklausų atvaizdavimas konsolėje</summary>
	public static bool Debug { get; set; } = Config.GetBool("Config","DebugDB",false);
	/// <summary>Duomenų bazės užklausa</summary>
	public string SQL { get; set; }
	/// <summary>Užklausos parametrai</summary>
	public DBParams Params { get; set; }

	private NpgsqlConnection? Conn { get; set; }
	private NpgsqlCommand? Cmd { get; set; }

	private NpgsqlCommand Command(string sql){
		Conn ??= new NpgsqlConnection(ConnString); Conn.Open();
		Cmd ??= new NpgsqlCommand(sql,Conn); Params?.Load(Cmd);
		return Cmd;
	}
	private async Task<NpgsqlCommand> CommandAsync(string sql, CancellationToken ct){
		Conn ??= new NpgsqlConnection(ConnString); await Conn.OpenAsync(ct);
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
	public int Execute() => Command(SQL).ExecuteNonQuery();

	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public async Task<int> Execute(CancellationToken ct) => await(await CommandAsync(SQL,ct)).ExecuteNonQueryAsync(ct);
	
	
	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public object? ExecuteScalar() => Command(SQL).ExecuteScalar();

	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public async Task<object?> ExecuteScalar(CancellationToken ct) => await(await CommandAsync(SQL,ct)).ExecuteScalarAsync(ct);


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