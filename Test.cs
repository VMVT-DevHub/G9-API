using System.Text.Json;
using Npgsql;


//TODO: connstring iš Doker ENV
//TODO: Duombazės pasiimimo/padavimo šablonas

namespace G9{

/// <summary>
/// a
/// </summary>
	public static class APIS{
		/// <summary>
		/// a
		/// </summary>
		/// <param name="ctx">b</param>
		/// <param name="ct">c</param>
		/// <returns>d</returns>
		public static async Task GoodMinimalApi(HttpContext ctx,CancellationToken ct){
			ctx.Response.ContentType="application/json";
			//await ctx.Response.WriteAsync("[{\"name\":\"Test Function\"}]",ct);


			var connString = "User ID=postgres;Password=postgres;Server=localhost;Port=5002;Database=postgres;Integrated Security=true;Pooling=true;";

			await using var conn = new NpgsqlConnection(connString);
			await conn.OpenAsync(ct);

			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug

			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);


			using var cmd = new NpgsqlCommand("SELECT id,name,title,gen_random_uuid() as uuid, now() as time FROM test", conn);

			using var reader = await cmd.ExecuteReaderAsync(ct);

			writer.WriteStartObject();
			writer.WritePropertyName("Data");
			writer.WriteStartArray();
			await Loop(reader,writer,ct);
			writer.WriteEndArray();
			writer.WriteEndObject();
		}
		/// <summary>
		/// a
		/// </summary>
		/// <param name="rdr">b</param>
		/// <param name="wrt">c</param>
		/// <param name="ct">d</param>
		/// <param name="flush">e</param>
		/// <returns>f</returns>

		public static async Task Loop(NpgsqlDataReader rdr, Utf8JsonWriter wrt,CancellationToken ct,int flush=20){
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
/// <summary>
/// a
/// </summary>
/// <param name="rdr">b</param>
/// <param name="wrt">c</param>
/// <param name="i">d</param>
/// <returns>e</returns>
		public static Action<int> GetAct(NpgsqlDataReader rdr, Utf8JsonWriter wrt, int i) {
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
}

