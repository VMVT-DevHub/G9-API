
namespace G9.Models;

/// <summary>Deklaruojamų metų sąrašas veikloms pagal ūkio subjektus</summary>
public class Veiklos {
	/// <summary>Ūkio subjektai priskirti prisijungusiam vartotojui.</summary>
	public List<BU>? BU { get; set; }
	/// <summary>Geriamo Vandens Tiekimo sistemų sąrašas</summary>
	public List<GVTS>? GVTS { get; set; }
	/// <summary>Deklaruojami metai</summary>
	public List<Year>? Year { get; set; }
}


/// <summary>Geriamojo vandens tiekimo sistemos ir deleguoti asmenys</summary>
public class Delegavimas {
	/// <summary>Juridinio asmens ūkio subjektai</summary>
	public List<BU>? BU { get; set; }
	/// <summary>Geriamojo vandens tiekimo sistemos priklausančios ūkio subjektams</summary>
	public List<GVTS>? GVTS { get; set; }
	/// <summary>Asmenys galintys deklaruoti GVTS rodmenis</summary>
	public List<Asmuo>? Year { get; set; }
}


/// <summary>Geriamojo vandens tiekimo sistemos deklaravimas už metus</summary>
public class Deklaravimas{
	public List<BU>? BU { get; set; }
	public List<GVTS>? GVTS { get; set; }
	public List<Year>? Year { get; set; }
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

/// <summary>Ūkio subjekto informacija</summary>
public class BU {
	/// <summary>Unikalus ūkio subjekto numeris</summary>
	public long ID { get; set; }
	/// <summary>Ūkio subjekto pavadinimas</summary>
	public string? Title { get; set; }
	/// <summary>Registruoto ūkio subjekto adresas</summary>
	public string? Address { get; set; }
}

/// <summary>Geriamojo vandens tiekimo sistemos informacija</summary>
public class GVTS {
	/// <summary>Unikalus numeris</summary>
	public long ID { get; set; }
	/// <summary>Ūkio subjekto numeris</summary>
	public long BU { get; set; }
	/// <summary>Pavadinimas</summary>
	public string? Title { get; set; }
	/// <summary>Adresas</summary>
	public string? Address { get; set; }

}

/// <summary>Deklaruojamų metų informacija</summary>
public class Year	{
	/// <summary>Unikalus numeris</summary>
	public int ID { get; set; }
	/// <summary>Gerimojo vandens tiekimo sistema</summary>
	public long GVTS { get; set; }
	/// <summary>Paskutinio keitimo data</summary>
	public DateTime? Modified { get; set; }
	/// <summary>Deklaravimo data</summary>
	public DateTime? Submitted { get; set; }
}

/// <summary>Asmens aprašas</summary>
public class Asmuo {
	/// <summary>Asmens kodas</summary>
	public long AK { get; set; }
	/// <summary>Vardas</summary>
	public string? FName { get; set; }
	/// <summary>Pavardė</summary>
	public string? LName { get; set; }
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
	public bool Tvirtinimas { get; set; }
}