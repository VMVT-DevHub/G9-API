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


app.MapGet("/api/deklar/{gvts}/{metai}",Deklaravimas.Get).Swagger(	
	"","Gaunamas deklaruojamų metų objektas"
).Response<G9.Models.Deklaravimas>(200,401,403,404).AddEndpointFilter(Require.Role);

app.MapPost("/api/deklar/{gvts}/{metai}",Deklaravimas.Set).Swagger(	
 	"","Daklaracijos duomenų įvedimas"
).Response<G9.Models.Deklaravimas>(200,422,401,403,404).AddEndpointFilter(Require.Role);

app.MapPost("/api/deklar/{deklaracija}",Deklaravimas.Submit).Swagger(	
 	"","Deklaruoti metus"
).Response<G9.Models.Deklaravimas>(200,422,401,403,404).AddEndpointFilter(Require.Role);



app.MapGet("/api/veiklos",Veiklos.Get).Swagger(	
	"","Gaunamos prisijungusio vartotojo veiklos ir deklaruojami metai"
).Response<G9.Models.Veiklos>(200,401,403).AddEndpointFilter(Require.Role);



app.MapGet("/api/rodikliai",Rodikliai.List).Swagger(	
	"","Gaunamas pilnas rodiklių sąrašas"
).Response<G9.Models.RodikliuSarasas>(200).AddEndpointFilter(Require.Login);




app.MapGet("/api/reiksmes/{deklaracija}",Reiksmes.Get).Swagger(	
	"","Gauti deklaruojamų rodiklių reikšmes"
).Response<G9.Models.ArrayModel<G9.Models.RodiklioReiksme>>(200,401,403,404).AddEndpointFilter(Require.Role);

app.MapPost("/api/reiksmes/{deklaracija}",Reiksmes.Set).Swagger(	
 	"", "Įrašomos rodiklių reikšmės"
).Produces(204).Errors(422,401,403,404).AddEndpointFilter(Require.Role);

app.MapDelete("/api/reiksmes/{deklaracija}",Reiksmes.Del).Swagger(	
 	"", "Pašalinti rodiklių reikšmes deklaracijoje"
).Produces(204).Errors(422,401,403,404).AddEndpointFilter(Require.Role);


app.Run();
