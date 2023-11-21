
namespace G9.Models;

/// <summary>Deklaruojamų metų sąrašas veikloms pagal ūkio subjektus</summary>
public class Veiklos {
	/// <summary>Ūkio subjektai priskirti prisijungusiam vartotojui.</summary>
	public List<JA>? JA { get; set; }
	/// <summary>Geriamo Vandens Tiekimo sistemų sąrašas</summary>
	public List<GVTS>? GVTS { get; set; }
	/// <summary>Deklaruojami metai</summary>
	public List<Deklaravimas>? Deklaracijos { get; set; }
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

/// <summary>Geriamojo vandens tiekimo sistemos informacija</summary>
public class GVTS {
	/// <summary>Unikalus numeris</summary>
	public long ID { get; set; }
	/// <summary>Ūkio subjekto numeris</summary>
	public long JA { get; set; }
	/// <summary>Pavadinimas</summary>
	public string? Title { get; set; }
	/// <summary>Adresas</summary>
	public string? Addr { get; set; }
}

/// <summary>Deklaruojamų metų informacija</summary>
public class Deklaravimas	{
	/// <summary>Gerimojo vandens tiekimo sistema</summary>
	public long GVTS { get; set; }
	/// <summary></summary>
	public int Metai { get; set; }
	/// <summary></summary>
	public string? Statusas { get; set; }
	/// <summary></summary>
	public double? Kiekis { get; set; }
	/// <summary></summary>
	public int? Vartotojai { get; set; }
	/// <summary>Deklaravimo data</summary>
	public DateTime? DeklarDate { get; set; }
	/// <summary>Deklaravęs vartotojas</summary>
	public string? DeklarUser { get; set; }
	/// <summary>Paskutinė redagavimo data</summary>
	public DateTime? RedagDate { get; set; }
	/// <summary>Paskurinis redagavęs vartotojas</summary>
	public string? RedagUser { get; set; }
}









/// <summary>Pilnas rodiklių sąrašas deklaruojamiems metams</summary>
public class Rodikliai : List<Rodiklis> {}

/// <summary>Rodiklio informacija</summary>
public class Rodiklis {
	/// <summary>Rodiklio unikalus numeris</summary>
	public int ID { get; set; }
	/// <summary>Rodiklio EU numeris</summary>
	public string? EID { get; set; }
	/// <summary>Rodiklio stebėsenos pavadinimas</summary>
	public string? Track { get; set; }
	/// <summary>Rodiklio grupė</summary>
	public string? Group { get; set; }
	/// <summary>Rodiklio pavadinimas</summary>
	public string? Title { get; set; }
	/// <summary>Minimalus tyrimų skaičius metams</summary>
	public int Freq { get; set; }
	/// <summary>Mažiausia rodiklio reikšmė</summary>
	public float Min { get; set; }
	/// <summary>Didžiausia rodiklio reikšmė</summary>
	public float Max { get; set; }
	/// <summary>Reikšmės mažiausias žingsnis</summary>
	public float Step { get; set; }
	/// <summary>Matavimo vienetai</summary>
	public string? Unit { get; set; }
	/// <summary>Aprašymas</summary>
	public string? Descr { get; set; }
}



















/// <summary>Geriamojo vandens tiekimo sistemos ir deleguoti asmenys</summary>
public class Delegavimas {
	/// <summary>Juridinio asmens ūkio subjektai</summary>
	public List<JA>? JA { get; set; }
	/// <summary>Geriamojo vandens tiekimo sistemos priklausančios ūkio subjektams</summary>
	public List<GVTS>? GVTS { get; set; }
	/// <summary>Asmenys galintys deklaruoti GVTS rodmenis</summary>
	public List<Asmuo>? Users { get; set; }
}








/// <summary>Asmens aprašas</summary>
public class Asmuo {
	/// <summary>Asmens kodas</summary>
	public long AK { get; set; }
	/// <summary>Vardas</summary>
	public string? FName { get; set; }
	/// <summary>Pavardė</summary>
	public string? LName { get; set; }
	/// <summary>GVTS administratorius</summary>
	public bool Admin { get; set; }
}

/// <summary>Prisijungusio vartotojo informacija</summary>
public class Vartotojas {
	/// <summary>Asmens Kodas</summary>
	public long AK { get; set; }
	/// <summary>Juridinio asmens kodas</summary>
	public long JA { get; set; }
	/// <summary>Juridinio asmens pavadinimas</summary>
	public string? Title { get; set; }
	/// <summary>Vardas</summary>
	public string? FName { get; set; }
	/// <summary>Pavardė</summary>
	public string? LName { get; set; }
	/// <summary>El paštas</summary>
	public string? Email { get; set; }
}

/// <summary>Deklaravimo informacija</summary>
public class Deklaruoti {
	/// <summary>Patvirtinti delkaruojamus metus</summary>
	public bool Tvirtinimas { get; set; }
}