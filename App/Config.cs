
namespace App;

/// <summary>Programos konfiguracija gaunama iš duomenų bazės</summary>
public static class Config{
	/// <summary>Sekančio konfiguracijos atnaujinimo laikas</summary>
	public static DateTime NextReload { get; private set; }
	
	/// <summary>Konfiguraciniai duomenys</summary>
	public static CfgData Data => Cache == null || NextReload<DateTime.UtcNow ? Reload() : Cache;

	/// <summary>Konfiguracinių duomenų laikinas saugijomas</summary>
	public static CfgData? Cache { get; set; }

	/// <summary>Atnaujinti konfiguraciją</summary>
	public static CfgData Reload(){
		NextReload=DateTime.UtcNow.AddMinutes(1);
		var ret = new CfgData();
		using var rdr = new DBExec("SELECT cfg_group,cfg_key,cfg_val,cfg_int,cfg_num,cfg_text,cfg_date FROM app.config;").GetReader();
		while(rdr.Read()){
			var vGrp = rdr.GetString(0);
			if(!ret.TryGetValue(vGrp, out var grp)) ret[vGrp]=grp=new();
			grp[rdr.GetString(1)]=new(){ Value=rdr.GetStringN(2), Int=rdr.GetLongN(3), Num=rdr.GetDoubleN(4), Text=rdr.GetStringN(5), Date=rdr.GetDateTimeN(6) };
		}
		Cache = ret;
		NextReload = DateTime.UtcNow.AddSeconds(GetLong("Config", "Reload", 300));
		return ret;
	}

	/// <summary>Gauti konfiguracinio įrašo reikšmę</summary>
	/// <param name="group">Įrašo grupė</param>
	/// <param name="key">Įrašo raktas</param>
	/// <param name="default">Standartinė reikšmė</param>
	/// <returns>Įrašo reikšmė</returns>
	public static string GetVal(string group, string key, string @default="") => Data.Get(group)?.Get(key)?.Value ?? @default;
	/// <summary>Gauti konfiguracinio įrašo reikšmę</summary>
	/// <param name="group">Įrašo grupė</param>
	/// <param name="key">Įrašo raktas</param>
	/// <returns>Įrašo reikšmė</returns>
	public static string? GetVal(string group, string key) => Data.Get(group)?.Get(key)?.Value;
	/// <summary>Gauti konfiguracinio įrašo tekstinę reikšmę</summary>
	/// <param name="group">Įrašo grupė</param>
	/// <param name="key">Įrašo raktas</param>
	/// <returns>Įrašo reikšmė</returns>
	public static string? GetText(string group, string key) => Data.Get(group)?.Get(key)?.Text;
	/// <summary>Gauti konfiguracinio įrašo skaitinę reikšmę</summary>
	/// <param name="group">Įrašo grupė</param>
	/// <param name="key">Įrašo raktas</param>
	/// <param name="default">Standartinė reikšmė</param>
	/// <returns>Įrašo skaitinė reikšmė</returns>
	public static int GetInt(string group, string key, int @default=0) => Convert.ToInt32(Data.Get(group)?.Get(key)?.Int ?? @default);

	/// <summary>Gauti konfiguracinio įrašo skaitinę reikšmę</summary>
	/// <param name="group">Įrašo grupė</param>
	/// <param name="key">Įrašo raktas</param>
	/// <param name="default">Standartinė reikšmė</param>
	/// <returns>Įrašo skaitinė reikšmė</returns>	
	public static long GetLong(string group, string key, int @default=0) => Data.Get(group)?.Get(key)?.Int ?? @default;

	/// <summary>Gauti konfiguracinio įrašo dvejetainę reikšmę</summary>
	/// <param name="group">Įrašo grupė</param>
	/// <param name="key">Įrašo raktas</param>
	/// <param name="default">Standartinė reikšmė</param>
	/// <returns>Įrašo skaitinė reikšmė</returns>	
	public static bool GetBool(string group, string key, bool @default=false) => (Data.Get(group)?.Get(key)?.Int ?? (@default?1:0))>0;

}


/// <summary>Konfiguracinio įrašo aprašas</summary>
public class CfgItem {
	/// <summary>Tekstinė reikšmė</summary>
	public string? Value { get; set; }
	/// <summary>Sveikas skaičius</summary>
	public long? Int { get; set; }
	/// <summary>Didelis skaičius</summary>
	public double? Num { get; set; }
	/// <summary>Neriboto ilgio tekstas</summary>
	public string? Text { get; set; }
	/// <summary>Data ir laikas</summary>
	public DateTime? Date { get; set; }
}


/// <summary>Konfiguracijos įrašų grupė</summary>
public class CfgGroup : Dictionary<string,CfgItem> {
	/// <summary>Gauti įrašą iš grupės</summary>
	/// <param name="key">Įrašo raktas</param>
	/// <returns>Konfiguracijos įrašas</returns>
	public CfgItem? Get(string key) => TryGetValue(key, out var itm) ? itm : null;
}

/// <summary>Visi konfiguracijos įrašai</summary>
public class CfgData : Dictionary<string,CfgGroup>{
	/// <summary>Gauti įrašų grupę</summary>
	/// <param name="group">Grupės raktas</param>
	/// <returns>Įrašų grupė</returns>
	public CfgGroup? Get(string group) => TryGetValue(group, out var itm) ? itm : null;
}