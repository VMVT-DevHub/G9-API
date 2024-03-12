using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Npgsql;


namespace App;

/// <summary>Duomenų bazės konfiguracija</summary>
public static class DBProps {

	/// <summary>Duombazės prisijungimas</summary>
	public static string ConnString { get; set; } = new NpgsqlConnectionStringBuilder(Startup.ConnStr) {
			MaxPoolSize = 20, // Maximum number of connections in the pool
			MinPoolSize = 5,  // Minimum number of connections in the pool
			ConnectionIdleLifetime = 300,  // Maximum time (in seconds) that a connection can be idle in the pool	
		}.ConnectionString;
}

/// <summary>Postgres konektorius</summary>
public class DBExec : IDisposable {
	private static long ExecCount { get; set; } 
	/// <summary>Duomenų bazės užklausų atvaizdavimas konsolėje</summary>
	public static bool Debug { get; set; } = Config.GetBool("Config","DebugDB",false);
	private long ExecID{ get; set; }
	/// <summary>Duomenų bazės užklausa</summary>
	public string SQL { get; set; }
	/// <summary>Užklausos parametrai</summary>
	public DBParams Params { get; set; }
	/// <summary>Naudoti tranzakciją</summary>
	public bool UseTransaction { get; set; }
	/// <summary>Prisijungimo tranzakcija</summary>
	public NpgsqlTransaction? Transaction { get; private set; }

	private NpgsqlConnection? Conn { get; set; }
	private NpgsqlCommand? Cmd { get; set; }


	private NpgsqlCommand Command(string sql){
		Conn ??= new NpgsqlConnection(DBProps.ConnString); Conn.Open();
		if(UseTransaction) Transaction??=Conn.BeginTransaction();
		Cmd = new NpgsqlCommand(sql,Conn,Transaction); Params?.Load(ExecID,Cmd);
		return Cmd;
	}

	private async Task<NpgsqlCommand> CommandAsync(string sql, CancellationToken ct){
		Conn ??= new NpgsqlConnection(DBProps.ConnString);
		if(Conn.State != System.Data.ConnectionState.Open) await Conn.OpenAsync(ct);
		if(UseTransaction) Transaction??=await Conn.BeginTransactionAsync(ct);
		Cmd = new NpgsqlCommand(sql,Conn,Transaction); Params?.Load(ExecID,Cmd);
		return Cmd;
	}

	/// <summary>Duomenų bazės prisijungimo sukurimas</summary>
	/// <param name="sql">Užklausa</param>
	public DBExec(string sql){ SQL=sql; Params = new(); ExecID=ExecCount++; }

	/// <summary>Duomenų bazės prisijungimo sukurimas</summary>
	/// <param name="sql">Užklausa</param>
	/// <param name="param">Parametras</param>
	/// <param name="value">Reikšmė</param>
	public DBExec(string sql, string param, object? value){ SQL=sql; Params = new(); Params.Add(param,value);  ExecID=ExecCount++;}

	/// <summary>Duomenų bazės prisijungimo sukurimas</summary>
	/// <param name="sql">Užklausa</param>
	/// <param name="param">Parametrai</param>
	public DBExec(string sql, params ValueTuple<string, object?>[] param){ SQL=sql; Params = new(param); ExecID=ExecCount++; }

	/// <summary>Duomenų bazės prisijungimo sukurimas</summary>
	/// <param name="sql">Užklausa</param>
	/// <param name="param">Parametrai</param>
	public DBExec(string sql, DBParams param){ SQL=sql; Params=param; ExecID=ExecCount++; }

	/// <summary>Gauti duomenų skaitytuvą</summary>
	/// <returns>NpgsqlDataReader</returns>
	public NpgsqlDataReader GetReader() => Command(SQL).ExecuteReader();

	/// <summary>Gauti duomenų skaitytuvą</summary>
	/// <returns>NpgsqlDataReader</returns>
	public async Task<NpgsqlDataReader> GetReader(CancellationToken ct) => await (await CommandAsync(SQL,ct)).ExecuteReaderAsync(ct);

	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public int Execute() { var ret = Command(SQL).ExecuteNonQuery(); if(!UseTransaction) Dispose(); return ret; }

	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public async Task<int> Execute(CancellationToken ct) { var ret = await(await CommandAsync(SQL,ct)).ExecuteNonQueryAsync(ct); if(!UseTransaction) Dispose(); return ret;}
	
	/// <summary>Vykdyti SQL užklausą</summary>
	/// <param name="sql"></param>
	/// <param name="ct"></param>
	/// <returns>Įrašų skaičius</returns>
	public async Task<int> Execute(string sql, CancellationToken ct) { var ret = await(await CommandAsync(sql,ct)).ExecuteNonQueryAsync(ct); if(!UseTransaction) Dispose(); return ret;}
	
	/// <summary>Vykdyti SQL užklausą</summary>
	/// <param name="sql"></param>
	/// <param name="param"></param>
	/// <param name="ct"></param>
	/// <returns>Įrašų skaičius</returns>
	public async Task<int> Execute(string sql, DBParams param, CancellationToken ct) { Params=param; var ret = await(await CommandAsync(sql,ct)).ExecuteNonQueryAsync(ct); if(!UseTransaction) Dispose(); return ret;}
	
	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public object? ExecuteScalar() { var ret = Command(SQL).ExecuteScalar(); if(!UseTransaction) Dispose(); return ret;}

	/// <summary>Vykdyti SQL užklausą grąžinant reikšmę</summary>
	/// <returns>Įrašų skaičius</returns>
	public T? ExecuteScalar<T>() { var ret = Command(SQL).ExecuteScalar(); if(!UseTransaction) Dispose(); return ret is T t ? t: default;}

	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public async Task<object?> ExecuteScalar(CancellationToken ct) { var ret = await(await CommandAsync(SQL,ct)).ExecuteScalarAsync(ct); if(!UseTransaction) Dispose(); return ret; }

	/// <summary>Vykdyti SQL užklausą</summary>
	/// <returns>Įrašų skaičius</returns>
	public async Task<T?> ExecuteScalar<T>(CancellationToken ct) { var ret = await(await CommandAsync(SQL,ct)).ExecuteScalarAsync(ct); if(!UseTransaction) Dispose(); return ret is T t ? t: default; }

	/// <summary>Gauti įrašus kaip masyvą</summary>
	/// <typeparam name="T">Įrašo formatas</typeparam>
	/// <param name="col">Įrašo stulpelis</param>
	/// <returns>Masyvas</returns>
	public List<T> GetList<T>(int col=0){
		using var rdr = GetReader();
		var ret = new List<T>();
		while(rdr.Read()) if(!rdr.IsDBNull(col)) ret.Add(rdr.GetFieldValue<T>(col));
		Dispose();return ret;
	}

	/// <summary>Gauti įrašus kaip masyvą</summary>
	/// <typeparam name="T">Įrašo formatas</typeparam>
	/// <param name="col">Įrašo stulpelis</param>
	/// <param name="ct"></param>
	/// <returns>Masyvas</returns>
	public async Task<List<T>> GetListAsync<T>(CancellationToken ct, int col=0){
		using var rdr =  await GetReader(ct);
		var ret = new List<T>();
		while(await rdr.ReadAsync(ct)) if(!rdr.IsDBNull(col)) ret.Add(await rdr.GetFieldValueAsync<T>(col));
		Dispose(); return ret;
	}

	// To detect redundant calls
	private bool IsDisposed;
	/// <summary>Duomenų bazės uždarymo metodas</summary>
	public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
	/// <summary>Duomenų bazės uždarymo metodas</summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing) {
		if (!IsDisposed) {
			if (disposing) {
				try {
					Cmd?.Dispose(); Cmd=null;
					Conn?.Dispose(); Conn=null;
					Transaction?.Dispose(); Transaction=null;
				} catch (Exception ex) {				
					Console.WriteLine($"[SQLError] [{ExecID}] Dispose  {ex.Message}");
					Console.WriteLine(ex.StackTrace);
				}

				//Dispose stuff;
			}
			IsDisposed = true;
		}
	}
}

/// <summary>Masinis duomenų įvedimas</summary>
public class DBBatch : IDisposable {	
	/// <summary>Duomenų bazės užklausų atvaizdavimas konsolėje</summary>
	public static bool Debug { get; set; } = Config.GetBool("Config","DebugDB",false);
	private NpgsqlConnection Conn { get; set; }
	private NpgsqlBinaryImporter? Wrt { get; set; }
	private NpgsqlBinaryImporter Writer => Wrt is null? Wrt=Conn.BeginBinaryImport($"COPY \"{TblSchema}\".\"{TblName}\" (\"{string.Join("\",\"",TblFields)}\") FROM STDIN (FORMAT BINARY);") : Wrt;
	private NpgsqlTransaction Tran { get; set; }
	private static long ExecCount { get; set; } 
	private long ExecID { get; set; }

	/// <summary>Eilučių įrašymo kiekis</summary>
	public long Total { get; set; }

	/// <summary>Lentelės įrašymas baigtas</summary>
	public bool Completed { get; private set; }
	private string TblSchema {get;set;}
	private string TblName {get;set;}
	private List<string> TblFields {get;set;}

	/// <summary>Inicijuojti duomenų perkėlimą</summary>
	/// <param name="table">Lentelės pavadinimas</param>
	/// <param name="columns">Stulpeliai</param>
	/// <param name="schema">Lentelės schema</param>
	public DBBatch(string table, List<string> columns, string schema = "public"){
		ExecID=ExecCount++; TblFields=columns; TblName=table; TblSchema=schema;
		Conn = new NpgsqlConnection(DBProps.ConnString); Conn.Open();
		Tran = Conn.BeginTransaction();
	}


	/// <summary>Pridėti įrašą</summary><param name="itm"></param>
	public void Add(params object?[] itm){		
		Writer.WriteRow(itm); Total++; 
	}
		/// <summary>Pridėti įrašą</summary><param name="ct"></param><param name="itm"></param>
	public async Task Add(CancellationToken ct, params object?[] itm){ await Writer.WriteRowAsync(ct, itm); Total++; }

	/// <summary>Užbaigti duomenų perkėlimą</summary>
	public void Commit(bool complete=false){ Writer.Complete(); Writer.Close(); Completed = true; if(complete) Complete(); }

	/// <summary>Užbaigti duomenų perkėlimą</summary>
	public async Task Commit(CancellationToken ct, bool complete=false){ await Writer.CompleteAsync(ct); await Writer.CloseAsync(ct); Completed = true; if(complete) await Complete(ct); }

	/// <summary>Užbaigti tranzakciją</summary>
	public void Complete(){ Tran.Commit(); }
	/// <summary>Užbaigti tranzakciją</summary>
	public async Task Complete(CancellationToken ct){ await Tran.CommitAsync(ct); }




	/// <summary>Vykdyti SQL užklausą</summary><param name="sql"></param><param name="param"></param><param name="ct"></param><returns>Įrašų skaičius</returns>
	public async Task<int> Execute(string sql, CancellationToken ct, params ValueTuple<string, object?>[]? param)  { 
		using var cmd = new NpgsqlCommand(sql, Conn, Tran);
		if(param is not null) new DBParams(param).Load(ExecID, cmd);
		return await cmd.ExecuteNonQueryAsync(ct);
	}

	/// <summary>Vykdyti SQL užklausą</summary><returns>Reikšmė</returns>
	public async Task<T?> ExecuteScalar<T>(string sql, CancellationToken ct, params ValueTuple<string, object?>[]? param) {
		using var cmd = new NpgsqlCommand(sql, Conn, Tran);
		if(param is not null) new DBParams(param).Load(ExecID, cmd);
		var ret = await cmd.ExecuteScalarAsync(ct);
		return ret is T t ? t: default; 
	}







	// To detect redundant calls
	private bool IsDisposed;
	/// <summary>Duomenų bazės uždarymo metodas</summary>
	public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
	/// <summary>Duomenų bazės uždarymo metodas</summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing) {
		if (!IsDisposed) {
			if (disposing) {
				try {
					if(!Completed){ Writer.Close(); Tran.Rollback(); }
					Writer.Dispose();
					Tran.Dispose();
					Conn.Dispose();
				} catch (Exception ex) {				
					Console.WriteLine($"[SQLTranError] Dispose  {ex.Message}");
					Console.WriteLine(ex.StackTrace);
				}
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



	/// <summary>Atiduoti užklausą kaip objektų masyvą į API</summary>
	/// <param name="query">DB užklausa</param>
	/// <param name="dbparams">Užklausos parametrai</param>
	/// <param name="wrt">Json writer</param>
	/// <param name="ct"></param>
	/// <param name="flush">atiduodamas įrašų kiekis</param>
	/// <returns></returns>
	public static async Task PrintObjectArray(string query, DBParams? dbparams,Utf8JsonWriter wrt, CancellationToken ct,int flush=20){
		using var db = new DBExec(query, dbparams??new());
		using var rdr = await db.GetReader(ct);
		wrt.WriteStartArray();
		await LoopObjects(rdr,wrt,ct,flush);
		wrt.WriteEndArray();
	}
	
	/// <summary>Atiduoti užklausą kaip masyvą į API</summary>
	/// <param name="query">DB užklausa</param>
	/// <param name="dbparams">Užklausos parametrai</param>
	/// <param name="wrt">Json writer</param>
	/// <param name="ct"></param>
	/// <param name="lookup">Lookup objektas</param>
	/// <param name="error">Kaidų sąrašas</param>
	/// <param name="flush">atiduodamas įrašų kiekis</param>
	/// <returns></returns>
	public static async Task<int> PrintArray(string query, DBParams? dbparams,Utf8JsonWriter wrt, CancellationToken ct,CachedLookup? lookup=null, object? error=null,int flush=20){
		using var db = new DBExec(query, dbparams??new());
		using var rdr = await db.GetReader(ct);
		wrt.WriteStartObject();
		wrt.WritePropertyName("Fields");
		wrt.WriteStartArray();
			var fct = rdr.FieldCount;
			for(var i=0; i<fct ;i++) wrt.WriteStringValue(rdr.GetName(i));
		wrt.WriteEndArray();
		wrt.WritePropertyName("Data");		
		wrt.WriteStartArray();
		var ret = await Loop(rdr,wrt,ct,flush);
		wrt.WriteEndArray();
		if(lookup is not null){
			wrt.WritePropertyName("Lookup");
			wrt.WriteRawValue(lookup.Json);
		}
		if(error is not null){
			wrt.WritePropertyName("Error");
			wrt.WriteRawValue(JsonSerializer.Serialize(error));
		}
		wrt.WriteEndObject();

		return ret;
	}

	/// <summary>Atiduoti užklausą sąrašą kaip masyvą</summary>
	/// <param name="query">DB užklausa</param>
	/// <param name="dbparams">Užklausos parametrai</param>
	/// <param name="wrt">Json writer</param>
	/// <param name="ct"></param>
	/// <param name="field">Stulpelio ID</param>
	/// <param name="flush">atiduodamas įrašų kiekis</param>
	/// <returns></returns>
	public static async Task PrintList(string query, DBParams? dbparams,Utf8JsonWriter wrt, CancellationToken ct,int field=0,int flush=200){
		using var db = new DBExec(query, dbparams??new());
		using var rdr = await db.GetReader(ct);
		var act = GetAct(rdr,wrt,0);
		var cnt=flush;
		wrt.WriteStartArray();
		while (await rdr.ReadAsync(ct)) { act(0); if(cnt--<1){ wrt.Flush(); cnt=flush; } }
		wrt.WriteEndArray();
	}

	/// <summary>Atiduoti masyvo objektus į API</summary>
	/// <param name="rdr"></param><param name="wrt"></param><param name="ct"></param>
	/// <param name="flush">atiduodamas įrašų kiekis</param>
	/// <returns></returns>
	public static async Task LoopObjects(NpgsqlDataReader rdr, Utf8JsonWriter wrt, CancellationToken ct, int flush=20){
		var act= new List<Action<int>>();
		var fct = rdr.FieldCount;
		for(var i =0; i<fct; i++) act.Add(GetActObject(rdr,wrt,i));
		var cnt=flush;
		while (await rdr.ReadAsync(ct)) {
			wrt.WriteStartObject();
			for(var i=0; i<fct ;i++) act[i](i);
			wrt.WriteEndObject();
			if(cnt--<1){ wrt.Flush(); cnt=flush; }
		}
	}
	
	/// <summary>Atiduoti masyvo duomenis į API</summary>
	/// <param name="rdr"></param><param name="wrt"></param><param name="ct"></param>
	/// <param name="flush">atiduodamas įrašų kiekis</param>
	/// <returns></returns>
	public static async Task<int> Loop(NpgsqlDataReader rdr, Utf8JsonWriter wrt, CancellationToken ct, int flush=20){
		var act= new List<Action<int>>();
		var fct = rdr.FieldCount;
		int ret = 0;
		for(var i =0; i<fct; i++) act.Add(GetAct(rdr,wrt,i));
		var cnt=flush;
		while (await rdr.ReadAsync(ct)) {
			wrt.WriteStartArray();
			for(var i=0; i<fct ;i++) act[i](i);
			wrt.WriteEndArray();
			if(cnt--<1){ wrt.Flush(); cnt=flush; }
			ret++;
		}
		return ret;
	}

	/// <summary>Gauti lookup reikšmes</summary>
	/// <param name="view"></param>
	/// <returns></returns>
	public static Dictionary<string,string> GetValues(string view) {
		var ret = new Dictionary<string,string>();
		using var db = new DBExec($"SELECT key,val FROM g9.{view};");
		using var rdr = db.GetReader();
		var isint = rdr.GetFieldType(0) == typeof(int);
		while(rdr.Read()) ret[(isint?rdr.GetIntN(0).ToString():rdr.GetStringN(0))??""]=rdr.GetStringN(1)??"";
		return ret;
	}
	
	/// <summary>Objekto duomenis į API</summary>
	/// <param name="rdr"></param><param name="wrt"></param><param name="ct"></param>
	/// <returns></returns>
	public static async Task GetObject(NpgsqlDataReader rdr, Utf8JsonWriter wrt, CancellationToken ct){
		var act= new List<Action<int>>();
		var fct = rdr.FieldCount;
		for(var i =0; i<fct; i++) act.Add(GetActObject(rdr,wrt,i));
		if (await rdr.ReadAsync(ct)) for(var i=0; i<fct ;i++) act[i](i);
	}


	/// <summary>Duomenų konvertavimo funkcija</summary>
	/// <param name="rdr">Data reader</param>
	/// <param name="wrt">JSON writer</param>
	/// <param name="i">Duomenų lauko ID</param>
	/// <returns>Funkcija</returns>
	private static Action<int> GetActObject(NpgsqlDataReader rdr, Utf8JsonWriter wrt, int i) {
		var tp= rdr.GetFieldType(i); var nm= rdr.GetName(i);
		if(tp==typeof(bool)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteBoolean(nm,rdr.GetBoolean(i));};
		else if(tp==typeof(byte)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetByte(i));};
		else if(tp==typeof(char)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetChar(i));};
		else if(tp==typeof(DateTime)) {
			var dtp = rdr.GetDataTypeName(i);
			if(dtp == "date") return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteString(nm,rdr.GetDateTime(i).ToString("yyyy-MM-dd"));};
			return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteString(nm,rdr.GetDateTime(i).ToString("yyyy-MM-ddTHH:mm:ssZ"));};
		}
		else if(tp==typeof(decimal)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetDecimal(i));};
		else if(tp==typeof(double)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetDouble(i));};
		else if(tp==typeof(float)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetFloat(i));};
		else if(tp==typeof(Guid)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteString(nm,rdr.GetGuid(i));};
		else if(tp==typeof(short)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetInt16(i));};
		else if(tp==typeof(int)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetInt32(i));};
		else if(tp==typeof(long)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetInt64(i));};
		else if(tp==typeof(string)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteString(nm,rdr.GetString(i));};
		else if(tp==typeof(TimeSpan)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else wrt.WriteNumber(nm,rdr.GetTimeSpan(i).Ticks);};
		else if(tp==typeof(Array)) return (i)=> { if(rdr.IsDBNull(i)) wrt.WriteNull(nm); else { 
			wrt.WritePropertyName(nm); wrt.WriteRawValue(JsonSerializer.Serialize(rdr.GetValue(i)));
		}};
		else return (i)=>{};
	} 

	/// <summary>Duomenų konvertavimo funkcija</summary>
	/// <param name="rdr">Data reader</param>
	/// <param name="wrt">JSON writer</param>
	/// <param name="i">Duomenų lauko ID</param>
	/// <returns>Funkcija</returns>
	private static Action<int> GetAct(NpgsqlDataReader rdr, Utf8JsonWriter wrt, int i) {
		var tp= rdr.GetFieldType(i);
		if(tp==typeof(bool)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteBooleanValue(rdr.GetBoolean(i));};
		else if(tp==typeof(byte)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteNumberValue(rdr.GetByte(i));};
		else if(tp==typeof(char)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteNumberValue(rdr.GetChar(i));};
		else if(tp==typeof(DateTime)) {
			var dtp = rdr.GetDataTypeName(i);
			if(dtp == "date") return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteStringValue(rdr.GetDateTime(i).ToString("yyyy-MM-dd"));};
			return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else {wrt.WriteStringValue(rdr.GetDateTime(i).ToString("yyyy-MM-ddTHH:mm:ssZ"));}};
		}
		else if(tp==typeof(decimal)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteNumberValue(rdr.GetDecimal(i));};
		else if(tp==typeof(double)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteNumberValue(rdr.GetDouble(i));};
		else if(tp==typeof(float)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteNumberValue(rdr.GetFloat(i));};
		else if(tp==typeof(Guid)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteStringValue(rdr.GetGuid(i));};
		else if(tp==typeof(short)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteNumberValue(rdr.GetInt16(i));};
		else if(tp==typeof(int)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteNumberValue(rdr.GetInt32(i));};
		else if(tp==typeof(long)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteNumberValue(rdr.GetInt64(i));};
		else if(tp==typeof(string)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteStringValue(rdr.GetString(i));};
		else if(tp==typeof(TimeSpan)) return (i)=>{if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteNumberValue(rdr.GetTimeSpan(i).Ticks);};
		else if(tp==typeof(Array)) return (i)=> { if(rdr.IsDBNull(i)) wrt.WriteNullValue(); else wrt.WriteRawValue(JsonSerializer.Serialize(rdr.GetValue(i)));};
		else return (i)=>{};
	} 
}

/// <summary>Duomenų bazės užklausos parametrai</summary>
public class DBParams {
	/// <summary>Parametrai</summary>
	public Dictionary<string, object?> Data { get; set; }
	/// <summary>Įkelti parametrus į užklausą</summary>
	/// <param name="id">Užklausos vykdymo ID</param>
	/// <param name="cmd">Duomenų bazės komanda</param>
	/// <param name="print">Spausdinti konsolėje</param>
	public void Load(long id, NpgsqlCommand cmd, bool print = false) { foreach (var i in Data) { if (!cmd.Parameters.Contains(i.Key)) { cmd.Parameters.AddWithValue(i.Key, i.Value ?? DBNull.Value); } }
		if (print || DBExec.Debug) { Console.WriteLine($"[SQL] [{id}] {cmd.CommandText} {(Data?.Count > 0 ? JsonSerializer.Serialize(Data) : "")}"); }
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



/// <summary>Dažnai naudojamų skaitinių reikšmių modelis</summary>
public class CachedLookup : Dictionary<string,Dictionary<string,string>> {
	/// <summary>Json formatas</summary>
	public string Json => Refresh().Cached??"";
	
	private string? Cached { get; set; }
	private DateTime Reload { get; set; }
	private string CfgName { get; set; }
	private Dictionary<string,string> Props { get;set; } 

	/// <summary>Inicijuojamas naujos Lookup reikšmės</summary>
	/// <param name="cfg">Konfiguracijos pavadinimas</param>
	/// <param name="pairs">Parametrai</param>
	public CachedLookup(string cfg, params ValueTuple<string, string>[] pairs){
		CfgName=cfg;
		Props= pairs.ToDictionary(x => x.Item1, x => x.Item2);
		Refresh(true);
	}

	/// <summary>Atnaujinti reikšmes</summary>
	/// <param name="force">Priverstinai atnaujinti</param>
	/// <returns>Reikšmės</returns>
	public CachedLookup Refresh(bool force=false) {
		if(force || Cached is null || Reload<DateTime.UtcNow){
			Reload = DateTime.UtcNow.AddSeconds(Config.GetInt("Cache",$"{CfgName}Values",300));
			Clear();
			foreach(var i in Props) this[i.Key]=DBExtensions.GetValues(i.Value);
			Cached = JsonSerializer.Serialize(this);
		}
		return this;
	}
}

/// <summary>Sukurti dažnai naudojamų objektų sąrašą</summary>
/// <typeparam name="T1">Sąrašo rakto formatas</typeparam>
/// <typeparam name="T2">Sąrašo formatas</typeparam>
/// <remarks>Inicijuojamas naujas objektas</remarks>
/// <param name="cfg">Konfiguracijos pavadinimas</param>
/// <param name="funct">Pildymo funkcija</param>
public class CachedLookup<T1,T2>(string cfg, Action<Dictionary<T1, T2>> funct) : Dictionary<T1,T2> where T1 : notnull {
	private DateTime Reload { get; set; }
	private Action<Dictionary<T1, T2>> Function { get; set; } = funct;
	/// <summary>Konfiguracijos pavadinimas</summary>
	public string CfgName { get; set; } = cfg;

	/// <summary>Atnaujinti duomenis</summary>
	/// <param name="force">Priverstinai atnaujinti</param><returns></returns>
	public CachedLookup<T1,T2> Refresh(bool force=false){
		if(force || Reload<DateTime.UtcNow){
			Reload = DateTime.UtcNow.AddSeconds(Config.GetInt("Cache",$"{CfgName}Values",300));
			Clear(); Function(this);
		}
		return this;
	}
}
