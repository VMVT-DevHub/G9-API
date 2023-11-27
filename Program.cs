using App.API;
using App.Auth;

var app = Startup.Build(args);

app.MapGet("/",()=>new G9.Models.Rodiklis()).ExcludeFromDescription();


app.MapGet("/api/user",Auth.Get).Swagger(
	"Informacija apie prisijungusį vartotoją",
    "Gaunamas objektas"
).Response<G9.Models.Vartotojas>(200,401).AddEndpointFilter(Require.Login);

app.MapGet("/api/login",Auth.Login).Swagger(
	"Prisijungti per VIISP",
    "Vartotojas peradresuojamas"
).Produces(302);

app.MapGet("/api/login/evartai",Auth.Evartai).Swagger(
	"VIISP autorizacijos grąžinimas",
    "Vartotojas peradresuojamas"
).Produces(302);



// app.MapGet("/api/deleg",Delegavimas.Get).Swagger(
// 	"Gauti visas prisijungusio juridinio asmens deleguotų asmenų sąrašą.",
//     "Gaunamas objektas"
// ).Produces<G9.Models.Delegavimas>(200).AddEndpointFilter(Require.Role);


// app.MapPost("/api/deleg/{gvts}",Delegavimas.Set).Swagger(	
// 	"Pridėti deleguojamą asmenį",
// 	"Gaunamas objektas"
// ).Produces(204);

// app.MapDelete("/api/deleg/{gvts}",Delegavimas.Del).Swagger(	
// 	"Trinti deleguojamą asmenį",
// 	"Gaunamas objektas"
// ).Produces(204);


app.MapGet("/api/declar/{gvts}/{metai}",Deklaravimas.Get).Swagger(	
	"Gauti deklaruojamų rodiklių sąrašą.",
	"Gaunamas objektas"
).Response<G9.Models.Deklaravimas>(200,401,403,404).AddEndpointFilter(Require.Role);

app.MapPost("/api/declar/{gvts}/{metai}",Deklaravimas.Set).Swagger(	
 	"Atnaujinti deklaruojamų metų detales",
 	"Tuščias atsakymas"
).Response<G9.Models.Deklaravimas>(200,422,401,403,404).AddEndpointFilter(Require.Role);

app.MapGet("/api/veiklos",Veiklos.Get).Swagger(	
	"Gauti visas prisijungusio vartotojo G9 veiklas.",
	"Gaunamas objektas"
).Response<G9.Models.Veiklos>(200,401,403).AddEndpointFilter(Require.Role);



/*
app.MapGet("/api/rodikliai",Rodikliai.List).Swagger(	
	"Gauti visus G9 rodiklius.",
	"Gaunamas objektas"
).Response<G9.Models.Rodikliai>(200);


app.MapGet("/api/rodiklis/{gvts}/{metai}",Rodikliai.Get).Swagger(	
	"Gauti deklaruojamus metinius rodiklius veiklai",
	"(Nenaudojamas)"
).Response<G9.Models.Rodikliai>(200);

app.MapPost("/api/rodiklis/{gvts}/{metai}",Rodikliai.Set).Swagger(	
	"Išsaugiti rodiklius veiklai",
	"Siunčiamas objektas"
).Response<G9.Models.Rodikliai>(200);

app.MapDelete("/api/rodiklis/{gvts}/{metai}",Rodikliai.Del).Swagger(	
	"Pašalinti rodiklį",
	""
).Produces(204);

*/
app.Run();
