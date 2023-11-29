
using App.API;

namespace G9.Models;

/// <summary>Deklaruojamų metų sąrašas veikloms pagal ūkio subjektus</summary>
public class Veiklos {
	/// <summary>Ūkio subjektai priskirti prisijungusiam vartotojui.</summary>
	public ArrayModel<JA>? JA { get; set; }
	/// <summary>Geriamo Vandens Tiekimo sistemų sąrašas</summary>
	public ArrayModel<GVTS>? GVTS { get; set; }
	/// <summary>Deklaruojami metai</summary>
	public ArrayModel<Deklaravimas>? Deklaracijos { get; set; }
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
	/// <summary>Deklaravimo data</summary>
	public DateTime? DeklarDate { get; set; }
	/// <summary>Deklaravęs vartotojas</summary>
	public string? DeklarUser { get; set; }
	/// <summary>Paskutinė redagavimo data</summary>
	public DateTime? RedagDate { get; set; }
	/// <summary>Paskurinis redagavęs vartotojas</summary>
	public string? RedagUser { get; set; }
}


/// <summary>Deklaravimo informacija</summary>
public class DeklaravimasSet {
	/// <summary>Deklaruojamo vandens kiekis m3/para</summary>
	public double? Kiekis { get; set; }
	/// <summary>Aptarnaujamų vartotojų skaičius</summary>
	public int? Vartotojai { get; set; }
}


/// <summary>Json masyvo modelis</summary>
/// <typeparam name="T"></typeparam>
public class ArrayModel<T> {
	/// <summary>Duomenų aprašas</summary>
	public T? Model { get; set; }
	/// <summary>Duomenų laukai</summary>
	public List<string>? Fields { get; set; }
	/// <summary>Duomenų masyvas</summary>
	public List<List<object>>? Data { get; set; }
	/// <summary>Duomenų skaitinės reikšmės</summary>
	public Dictionary<string,Dictionary<string,string>>? Lookup { get; set; }
}


/// <summary>Visų galimų rodiklių sąrašas</summary>
public class RodikliuSarasas {
	/// <summary>Rodikliai</summary>
	public ArrayModel<Rodiklis>? Rodikliai { get; set; }
	/// <summary>Dažnumo grupės</summary>
	public ArrayModel<Daznumas>? Daznumas { get; set; }
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


