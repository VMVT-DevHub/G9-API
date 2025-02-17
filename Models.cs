using App;
using Microsoft.AspNetCore.Http.HttpResults;

namespace G9.Models;

/// <summary>Deklaruojamų metų sąrašas veikloms pagal ūkio subjektus</summary>
public class Veiklos {
	/// <summary>Ūkio subjektai priskirti prisijungusiam vartotojui.</summary>
	public ArrayModel<JADetails>? JA { get; set; }
	/// <summary>Geriamo Vandens Tiekimo sistemų sąrašas</summary>
	public ArrayModel<GVTS>? GVTS { get; set; }
//	/// <summary>Deklaruojami metai</summary>
//	public ArrayModelA<Deklaracija>? Deklaracijos { get; set; }
}


/// <summary>Ūkio subjekto informacija</summary>
public class JA {
	/// <summary>Unikalus ūkio subjekto numeris</summary>
	public long ID { get; set; }
	/// <summary>Ūkio subjekto pavadinimas</summary>
	public string? Title { get; set; }
	/// <summary>Registruoto ūkio subjekto adresas</summary>
	public string? Addr { get; set; }
}

/// <summary>Ūkio subjekto detali informacija</summary>
public class JADetails : JA {
	/// <summary>Kontaktinio asmens vardas</summary>
	public string? KontaktaiVardas { get; set; }
	/// <summary>Kontaktinio asmens pavardė</summary>
	public string? KontaktaiPavarde { get; set; }
	/// <summary>Kontaktinio asmens el.paštas</summary>
	public string? KontaktaiEmail { get; set; }
	/// <summary>Kontaktinio asmens telefono nr.</summary>
	public string? KontaktaiPhone { get; set; }
}

/// <summary>Ūkio subjekto detalių keitimas</summary>
public class JADetailsSet {
	/// <summary>Unikalus ūkio subjekto numeris</summary>
	public long ID { get; set; }
	/// <summary>Kontaktinio asmens vardas</summary>
	public string? KontaktaiVardas { get; set; }
	/// <summary>Kontaktinio asmens pavardė</summary>
	public string? KontaktaiPavarde { get; set; }
	/// <summary>Kontaktinio asmens el.paštas</summary>
	public string? KontaktaiEmail { get; set; }
	/// <summary>Kontaktinio asmens telefono nr.</summary>
	public string? KontaktaiPhone { get; set; }
}

/// <summary>Geriamojo vandens tiekimo sistemos informacija</summary>
public class GVTS {
	/// <summary>Unikalus numeris</summary>
	public long ID { get; set; }
	/// <summary>Ūkio subjekto numeris</summary>
	public long JA { get; set; }
	/// <summary>Pavadinimas</summary>
	public string? GVTOT { get; set; }
	/// <summary>Pavadinimas</summary>
	public string? Title { get; set; }
	/// <summary>Adresas</summary>
	public string? Addr { get; set; }
}


/// <summary>Deklaruojamų metų informacija</summary>
public class Deklaracija {
	/// <summary>Deklaracijos ID</summary>
	public int ID { get; set; }
	/// <summary>Gerimojo vandens tiekimo sistema</summary>
	public long GVTS { get; set; }
	/// <summary>Deklaruojami metai</summary>
	public int Metai { get; set; }
	/// <summary>Deklaravimo stebėsena</summary>
	public int Stebesenos { get; set; }
	/// <summary>Deklaravimo statusas</summary>
	public int Statusas { get; set; }
	/// <summary>Deklaruojamo vandens kiekis m3/para</summary>
	public double? Kiekis { get; set; }
	/// <summary>Aptarnaujamų vartotojų skaičius</summary>
	public int? Vartotojai { get; set; }
	/// <summary>Vandens ruošimui naudojamos medžiagos</summary>
	public bool? VanduoRuosiamas { get; set; }
	/// <summary>Vandens ruošimui naudojamos medžiagos</summary>
	public List<int>? RuosimoMedziagos { get; set; }
	/// <summary>Vandens ruošimo būdai</summary>
	public List<int>? RuosimoBudai { get; set; }

	/// <summary>Kontaktinio asmens vardas</summary>
	public string? KontaktaiVardas { get; set; }
	/// <summary>Kontaktinio asmens pavardė</summary>
	public string? KontaktaiPavarde { get; set; }
	/// <summary>Kontaktinio asmens el.paštas</summary>
	public string? KontaktaiEmail { get; set; }
	/// <summary>Kontaktinio asmens telefono nr.</summary>
	public string? KontaktaiPhone { get; set; }

	/// <summary>Deklaravimo data</summary>
	public DateTime? DeklarDate { get; set; }
	/// <summary>Deklaravęs vartotojas</summary>
	/// <example>Vardas Pavardė</example>
	public string? DeklarUser { get; set; }
	/// <summary>Paskutinė redagavimo data</summary>
	public DateTime? RedagDate { get; set; }
	/// <summary>Paskurinis redagavęs vartotojas</summary>
	/// <example>Vardas Pavardė</example>
	public string? RedagUser { get; set; }
}

/// <summary>Deklaruojamų metų masyvo modelis</summary>
public class DeklaracijaGet : ArrayModelA<Deklaracija> {}


/// <summary>Deklaravimo informacija</summary>
public class DeklaracijaSet {
	/// <summary>Deklaruojamo vandens kiekis m3/para</summary>
	public double? Kiekis { get; set; }
	/// <summary>Aptarnaujamų vartotojų skaičius</summary>
	public int? Vartotojai { get; set; }
	/// <summary>Vandens ruošimui naudojamos medžiagos</summary>
	public bool? VanduoRuosiamas { get; set; }
	/// <summary>Vandens ruošimui naudojamos medžiagos</summary>
	public List<int>? RuosimoMedziagos { get; set; }
	/// <summary>Vandens ruošimo būdai</summary>
	public List<int>? RuosimoBudai { get; set; }
	/// <summary>Kontaktinio asmens vardas</summary>
	public string? KontaktaiVardas { get; set; }
	/// <summary>Kontaktinio asmens pavardė</summary>
	public string? KontaktaiPavarde { get; set; }
	/// <summary>Kontaktinio asmens el.paštas</summary>
	public string? KontaktaiEmail { get; set; }
	/// <summary>Kontaktinio asmens telefono nr.</summary>
	public string? KontaktaiPhone { get; set; }

}


/// <summary>Json masyvo modelis su Lookup reikšmėmis</summary>
/// <typeparam name="T"></typeparam>
public class ArrayModelA<T> : ArrayModel<T> {
	/// <summary>Duomenų skaitinės reikšmės</summary>
	/// <example>{"Field":{"1":"Value1","2":"Value2"}}</example>
	public Dictionary<string,Dictionary<string,string>>? Lookup { get; set; }
}

/// <summary>Json masyvo modelis su Kalidos reikšmėmis</summary>
/// <typeparam name="T"></typeparam>
public class ArrayModelB<T> : ArrayModel<T> {
	/// <summary>Duomenų suvedimo klaidos</summary>
	public List<Klaida>? Errors { get; set; }
}

/// <summary>Deklaravimo klaida</summary>
public class Klaida {
	/// <summary>Duomenų skaitinės reikšmės</summary>
	public long ID { get; set; }
	/// <summary>Klaidos aprašymas</summary>
	public string? Message { get; set; }
	/// <summary>Klaidos konstruktorius</summary>
	public Klaida(){}
	/// <summary>Klaidos konstruktorius</summary>
	public Klaida(long id, string msg){ ID=id; Message=msg; }
}

/// <summary>Json masyvo modelis</summary>
/// <typeparam name="T"></typeparam>
public class ArrayModel<T> {
	/// <summary>Duomenų aprašas (pavizdys)</summary>
	public T? ModelExample { get; set; }
	/// <summary>Duomenų laukai</summary>
	/// <example>["ID","Title"]</example>
	public List<string>? Fields { get; set; }
	/// <summary>Duomenų masyvas</summary>
	/// <example>[[1,"Data1"],[2,"Data2"]]</example>
	public List<List<object>>? Data { get; set; }
}



/// <summary>Visų galimų rodiklių sąrašas</summary>
public class RodikliuSarasas {
	/// <summary>Rodikliai</summary>
	public ArrayModelA<Rodiklis>? Rodikliai { get; set; }
	/// <summary>Dažnumo grupės</summary>
	public ArrayModelA<Daznumas>? Daznumas { get; set; }
	/// <summary>Rodiklių sąrašas stebėsenoms</summary>
	public List<Stebesenos>? Stebesenos { get; set; }
}

/// <summary>Stebėsenų informacija</summary>
public class Stebesenos {
	/// <summary>Galimų rodiklių sąrašas stebėsenoms</summary>
	public int ID { get; set; }
	/// <summary>Stebėsenų padavinimas</summary>
	public string? Pavadinimas { get; set; }
	/// <summary>Galimų rodiklių sąrašas</summary>
	public List<int>? Rodikliai { get; set; }
}


/// <summary>Rodiklio informacija</summary>
public class Rodiklis {
	/// <summary>Rodiklio unikalus numeris</summary>
	public int ID { get; set; }
	/// <summary>Rodiklio grupė</summary>
	public int Grupe { get; set; }
	/// <summary>Rodiklio EU numeris</summary>
	public string? Kodas { get; set; }
	/// <summary>Rodiklio pavadinimas</summary>
	public string? Pavadinimas { get; set; }
	/// <summary>Minimalus tyrimų skaičius metams</summary>
	public string? Daznumas { get; set; }
	/// <summary>Mažiausia rodiklio reikšmė</summary>
	public float Min { get; set; }
	/// <summary>Didžiausia rodiklio reikšmė</summary>
	public float Max { get; set; }
	/// <summary>Reikšmės mažiausias žingsnis</summary>
	public float Step { get; set; }
	/// <summary>Matavimo vienetai</summary>
	public string? Vnt { get; set; }
	/// <summary>Aprašymas</summary>
	public string? Aprasymas { get; set; }
}

/// <summary>Rodiklių deklaravimo dažnumas metams</summary>
public class Daznumas {

}

/// <summary>Rodiklio delkaravimo reikšmė</summary>
public class RodiklioReiksme {
	/// <summary>Reikšmės identifikatorius</summary>
	/// <example>123</example>
	public long ID { get; set; }
	/// <summary>Deklaracijos identifikatorius</summary>
	/// <example>12</example>
	public int Deklaracija { get; set; }
	/// <summary>Rodiklio identifikatorius</summary>
	/// <example>23</example>
	public int Rodiklis { get; set; }
	/// <summary>Mėginio paimimo data</summary>
	public DateOnly Data { get; set; }
	/// <summary>Rodiklio reikšmė</summary>
	/// <example>23.4</example>
	public double Reiksme { get; set; }
}


/// <summary>Rodiklio reikšmės įvedimas</summary>
public class RodiklisSet {
	/// <summary>Rodiklio identifikatorius</summary>
	/// <example>1</example>
	public int Rodiklis { get; set; }
	/// <summary>Mėginio paimimo data</summary>
	public DateOnly Data { get; set; }
	/// <summary>Rodiklio reikšmė</summary>
	/// <example>23.4</example>
	public double Reiksme { get; set; }
}

/// <summary>Deklaravimo rezultatas</summary>
public class Deklaravimas {
	/// <summary>Deklaravimo statusas</summary>
	public string? Statusas { get; set; }
	/// <summary>Nepatvirtintų neatitikčių indikatorius</summary>
	public bool Klaida => Kartojasi + Trukumas + Virsijimas>0;
	/// <summary>Nepatvirtinti trūkumo neatitiktys</summary>
	public int Trukumas { get; set; }
	/// <summary>Nepatvirtinti rodiklių pasikartojimai</summary>
	public int Kartojasi { get; set; }
	/// <summary>Nepatvirtinti viršijimo neatitiktys</summary>
	public int Virsijimas { get; set; }
}

/// <summary>Deklaravimo pateikimas ir neatitikčių tvirtinimas</summary>
public class Neatitiktys {
	/// <summary>Rodiklių suvedimo trūkumai</summary>
	public ArrayModelB<ValidTrukumas>? Trukumas { get; set; }
	/// <summary>Besikartojantys rodiklių duomenys</summary>
	public ArrayModelB<ValidKartojasi>? Kartojasi { get; set; }
	/// <summary>Rodiklių viršijimas</summary>
	public ArrayModelB<ValidVirsijimas>? Virsijimas { get; set; }
}

/// <summary>Deklaravimo neatitikciu tipai</summary>
public enum NeatitikciuTipas {
	/// <summary>Trukstami suvedimai</summary>
	Trukumas, 
	/// <summary>Besikartojantys įvedimai</summary>
	Kartojasi, 
	/// <summary>Viršijamų rodiklių</summary>
	Virsijimas
}


/// <summary>Rodiklio reikšmių trūkumo validacija</summary>
public class ValidTrukumas {
	/// <summary>Validacijos identifikatorius</summary>
	public int ID { get; set; }
	/// <summary>Rodiklio identifikatorius</summary>
	public int Rodiklis { get; set; }
	/// <summary>Suvestas tyrimų skaičius</summary>
	public int Suvesta { get; set; }
	/// <summary>Reikiamas tyrimų skaičius</summary>
	public int Reikia { get; set; }
	/// <summary>Deklaruojančio asmens patvirtinimas</summary>
	public bool? Patvirtinta { get; set; }
	/// <summary>Pastabos</summary>
	public string? Pastabos { get; set; }
}

/// <summary>Pasikartojančių rodmenų validacija</summary>
public class ValidKartojasi {
	/// <summary>Validacijos identifikatorius</summary>
	public int ID { get; set; }
	/// <summary>Rodiklio identifikatorius</summary>
	public int Rodiklis { get; set; }	
	/// <summary>Suvesto rodiklio data</summary>
	public DateOnly Data { get; set; }
	/// <summary>Suvesto rodiklio reikšmė</summary>
	public double Reiksme { get; set; }
	/// <summary>Nereikšmingas viršijimas</summary>
	public bool? Patvirtinta { get; set; }
	/// <summary>String</summary>
	public string? Pastabos { get; set; }
}


/// <summary>Rodiklio viršyjimo pagrindimas</summary>
public class ValidVirsijimas {
	/// <summary>Validacijos identifikatorius</summary>
	public int ID { get; set; }
	/// <summary>Rodiklio identifikatorius</summary>
	public int Rodiklis { get; set; }
	/// <summary></summary>
	public DateOnly Nuo { get; set; }
	/// <summary></summary>
	public DateOnly Iki { get; set; }
	/// <summary>Maksimali viršyjimo reikšmė laikotarpiui</summary>
	public double Max { get; set; }
	/// <summary>Patvirtinimui reikalinga suvesti papildomą informaciją</summary>
	public bool Detales { get; set; }
	/// <summary>Nereikšmingas viršijimas</summary>
	public bool? Nereiksmingas { get; set; }
	/// <summary>Nereikšmingo viršijimo pagrindimas</summary>
	public string? NereiksmApras { get; set; }
	/// <summary>Viršijimo paveiktų žmonių skaičius</summary>
	public int? Zmones { get; set; }
	/// <summary>Mėginių ėmimo vietos tipas</summary>
	public int? Tipas { get; set; }
	/// <summary>Nustatyta vertė žemiau nei LOQ</summary>
	public bool? LOQVerte { get; set; }
	/// <summary>Kiekybinio nustatymo ribos LOQ reikšmė	</summary>
	public double? LOQReiksme { get; set; }
	/// <summary>Stebėjimo statusas</summary>
	public int? Statusas { get; set; }
	/// <summary>Neatitikimas patvirtintas</summary>
	public bool Patvirtinta { get; set; }
	/// <summary>Pastabos</summary>
	public string? Pastabos { get; set; }
	/// <summary>Virsijimo priežastis</summary>
	public int? Priezastis { get; set; }
	/// <summary>Taikomasis veiksmas</summary>
	public string? Veiksmas { get; set; }
	/// <summary>Taikomojo veiksmo pradžia</summary>
	public DateOnly Pradzia { get; set; }
	/// <summary>Taikomojo veiksmo pabaiga</summary>
	public DateOnly Pabaiga { get; set; }
}


/// <summary>Deklaravimo neatitikčių patvirtinimas</summary>
public class NeatitiktysSet {	
	/// <summary>Rodiklių suvedimo trūkumai</summary>
	public List<DeklarValidTrukumas>? Trukumas { get; set; }
	/// <summary>Besikartojantys rodiklių duomenys</summary>
	public List<DeklarValidKartojasi>? Kartojasi { get; set; }
	/// <summary>Rodiklių viršijimas</summary>
	public List<DeklarValidVirsijimas>? Virsijimas { get; set; }

}


/// <summary>Deklaruojamo rodiklio reikšmių kartojimosi patvirtinimas</summary>
public class DeklarValidKartojasi {
	/// <summary>Validacijos identifikatorius</summary>
	public int ID { get; set; }
	/// <summary></summary>
	public bool Patvirtinta { get; set; }
	/// <summary></summary>
	public string? Pastabos { get; set; }
}

/// <summary>Deklaruojamo rodiklio reikšmių kartojimosi patvirtinimas</summary>
public class DeklarValidTrukumas {
	/// <summary>Validacijos identifikatorius</summary>
	public int ID { get; set; }
	/// <summary>Taikomas kitas dažnumas</summary>
	public bool KitasDaznumas { get; set; }
	/// <summary></summary>
	public bool Patvirtinta { get; set; }
	/// <summary></summary>
	public string? Pastabos { get; set; }
}


/// <summary>Deklaruojamo rodiklio reikšmių viršijimo patvirtinimas</summary>
public class DeklarValidVirsijimas{
	/// <summary>Validacijos identifikatorius</summary>
	public int ID { get; set; }	
	/// <summary>Nereikšmingas viršijimas</summary>
	public bool? Nereiksmingas { get; set; }
	/// <summary>Nereikšmingo viršijimo pagrindimas</summary>
	public string? NereiksmApras { get; set; }
	/// <summary>Viršijimo paveiktų žmonių skaičius</summary>
	public int? Zmones { get; set; }
	/// <summary>Mėginių ėmimo vietos tipas</summary>
	public int? Tipas { get; set; }
	/// <summary>Nustatyta vertė žemiau nei LOQ</summary>
	public bool? LOQVerte { get; set; }
	/// <summary>Kiekybinio nustatymo ribos LOQ reikšmė	</summary>
	public double? LOQReiksme { get; set; }
	/// <summary>Stebėjimo statusas</summary>
	public int? Statusas { get; set; }
	/// <summary></summary>
	public bool Patvirtinta { get; set; }
	/// <summary></summary>
	public string? Pastabos { get; set; }
	/// <summary>Virsijimo priežastis</summary>
	public int? Priezastis { get; set; }
	/// <summary>Taikomasis veiksmas</summary>
	public string? Veiksmas { get; set; }
	/// <summary>Taikomojo veiksmo pradžia</summary>
	public DateOnly Pradzia { get; set; }
	/// <summary>Taikomojo veiksmo pabaiga</summary>
	public DateOnly Pabaiga { get; set; }

}


/// <summary>Reikalaujamas suvedimų rodiklių kiekis deklaracijai</summary>
public class ReikSuvedimai {
	/// <summary>Rodiklio identifikatorius</summary>
	public int Rodiklis { get; set; }
	/// <summary>Reikiamas tyrimų skaičius</summary>
	public int Reikia { get; set; }
}



/// <summary>Prisijungusio vartotojo informacija</summary>
public class Vartotojas {
	/// <summary>Vartotojo ID</summary>
	public Guid ID { get; set; }
	/// <summary>Vardas</summary>
	/// <example>Vardas</example>
	public string? FName { get; set; }
	/// <summary>Pavardė</summary>
	/// <example>Pavardė</example>
	public string? LName { get; set; }
	/// <summary>El paštas</summary>
	/// <example>info@vmvt.lt</example>
	public string? Email { get; set; }
	/// <summary>Telefonas</summary>
	/// <example>+37060000001</example>
	public string? Phone { get; set; }
	/// <summary>Sesijos pratęsimo laikas</summary>
	/// <example>2024-01-02T12:20:00Z</example>
	public DateTime SessionExtend { get; set; }
	/// <summary>Sessijos galiojimo pabaiga</summary>
	/// <example>2024-01-02T12:30:00Z</example>
	public DateTime SessionExpire { get; set; }
	/// <summary>Juridinio asmens duomenys</summary>
	public JA? JA { get; set; }
	/// <summary>Vartotojo GVTS rolės</summary>
	public List<int>? Roles { get; set; }
	/// <summary>Administruojami GVTS</summary>
	public List<int>? Admin { get; set; }
}


/// <summary>GVTS ir jų delegavimas</summary>
public class Delegavimas{
	/// <summary>Geriamo vandens tiekimo sistemos</summary>
	public ArrayModel<GVTS>? GVTS { get; set; }
	/// <summary>Deleguoti asmenys</summary>
	public ArrayModel<User>? Users { get; set; }
}

/// <summary>Deleguotas asmuo</summary>
public class User {
	/// <summary>Geriamo vandens tiekimo sistemos identifikatorius</summary>
	public long GVTS { get; set; }
	/// <summary>Deleguojamo asmens kodas</summary>
	public Guid ID { get; set; }
	/// <summary>Deleguojamo asmens vardas</summary>
	/// <example>Vardas</example>
	public string? FName { get; set; }
	/// <summary>Deleguojamo asmens pavardė</summary>
	/// <example>Pavardė</example>
	public string? LName { get; set; }
	/// <summary>Administratoriaus teisė</summary>
	public bool Admin { get; set; }
}


/// <summary>Asmens delegavimas</summary>
public class DelegavimasSet {
	/// <summary>Deleguojamo asmens kodas</summary>
	/// <example>90102031234</example>
	public long AK { get; set; }
	/// <summary>Deleguojamo asmens vardas</summary>
	/// <example>Vardas</example>
	public string? FName { get; set; }
	/// <summary>Deleguojamo asmens pavardė</summary>
	/// <example>Pavardė</example>
	public string? LName { get; set; }
	/// <summary>Administratoriaus teisė</summary>
	public bool Admin { get; set; }
}


/// <summary>Deklaracijos rodiklių suvedimo reikšmės</summary>
public class RodiklioSuvedimas {
	/// <summary>Rodiklio reikšmės ID</summary><example>1000</example>
	public long ID { get; set; }
	/// <summary>Rodiklio reikšmių suvedimo ID</summary><example>200</example>
	public long Suvedimas { get; set; }
	/// <summary>Rodiklio Kodas</summary><example>CAS_11-22-3</example>
	public string? Kodas { get; set; }
	/// <summary>Suvesto rodiklio data</summary><example>2024-01-02</example>
	public DateOnly Data { get; set; }
	/// <summary>Suvesto rodiklio reikšmė</summary><example>1.23</example>
	public double Reiksme { get; set; }
}

/// <summary>Reikšmių suvedimo rezultatas</summary>
public class ReiksmiuSuvedimasResult {
	/// <summary>Deklaracijos ID</summary>
	public int Deklaracija { get; set; }
	/// <summary>Suvedimo ID</summary>
	public long Suvedimas { get; set; }
	/// <summary>Suvestos reikšmės</summary>
	public long Reiksmes { get; set; }
}

/// <summary>Reikšmių trynimo rezultatas</summary>
public class ReiksmiuTrynimasResult {
	
	/// <summary>Deklaracijos ID</summary>
	public int Deklaracija { get; set; }
	/// <summary>Istrintų reikšmių skaičius</summary>
	public long Istrinta { get; set; }
}




/// <summary>GVTS API prisijungimų raktai</summary>
public class ApiKeys{
	/// <summary>Geriamo vandens tiekimo sistemos deklaracijos</summary>
	public ArrayModelA<APIDeklar>? Deklaracijos { get; set; }
	/// <summary>Deleguoti asmenys</summary>
	public ArrayModel<APIKey>? Raktai { get; set; }
}

/// <summary>Autorizacijos rakto struktūros modelis</summary>
public class APIKey {
	/// <summary>API rakto identifikatorius</summary>
	public Guid RaktoID { get; set; }
	/// <summary>Deklaracijos ID</summary>
	public int Deklaracija { get; set; } 
	/// <summary>Geriamo vandens tiekimo sistemos ID</summary>
	public long GVTS { get; set; }
	/// <summary>Rakto galiojimo pabaigos data</summary>
	public DateOnly GaliojaIki { get; set; }
	/// <summary>Rakto sukūrimo data</summary>
	public DateTime Sukurtas { get; set; }
	/// <summary>Vartotojas sukūręs prisijungimo raktą</summary>
	public string? Autorius { get; set; }
}

/// <summary>GVTS deklaracijos modelis autorizacijos raktui</summary>
public class APIDeklar {
	/// <summary>Deklaracija</summary>
	public long ID { get; set; }
	/// <summary>Deklaruojami metai</summary>
	public int Metai { get; set; }
	/// <summary>Stebėsenų ID</summary>
	public int Stebesenos { get; set; }
	/// <summary>Deklaracijos statusas</summary>
	public int Statusas { get; set; }
}

/// <summary>Deklaracijos rakto sukūrimas</summary>
public class APIKeyAdd {
	/// <summary>Deklaracijos ID</summary>
	public int Deklaracija { get; set; }
	/// <summary>Maksimali APIRakto galiojimo data</summary>
	public DateOnly? GaliojaIki { get; set; }
}

/// <summary>Deklaracijos rakto sukūrimo atsakas</summary>
public class APIKeyData {
	/// <summary>API autorizacijos rakto ID</summary>
	public Guid? RaktoID { get; set; }
	/// <summary>API autorizacijos raktas</summary>
	public string? Raktas { get; set; }
	/// <summary>Deklaracijos ID</summary>
	public int Deklaracija { get; set; }
	/// <summary>Maksimali APIRakto galiojimo data</summary>
	public long GVTS { get; set; }
	/// <summary>Prieigos galiojimo pabaigos data</summary>
	public DateOnly? GaliojaIki { get; set; }
}

/// <summary>Autorizacijos rakto ištrynimo atsakas</summary>
public class APIKeyDel {
	/// <summary></summary>
	public bool Ištrinta { get; set; }
}