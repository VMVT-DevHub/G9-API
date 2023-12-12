using App.API;
var app = Startup.Build(args);

app.MapGet("/",()=>"").ExcludeFromDescription();

app.MapGet("/auth/login",Auth.Login).Swagger("", "Vartotojas peradresuojamas į VIISP prisijungimą").Produces(302);
app.MapGet("/auth/logout",Auth.Logout).Swagger("", "Vartotojo atsijungimas nuo sistemos.").Produces(302);
app.MapGet("/auth/login/evartai",Auth.Evartai).ExcludeFromDescription();
app.MapGet("/api/user",Auth.Get).Swagger("","Informacija apie prisijungusį vartotoją").Response<G9.Models.Vartotojas>(200,401).RequireLogin();

// Teisių delegavimas
app.MapGet("/api/deleg",Delegavimas.Get).Swagger("","Gauti visas prisijungusio asmens administruojamų GVTS sąrašą.").Response<G9.Models.Delegavimas>(200,422,401,403,404).RequireRole();
app.MapPost("/api/deleg/{gvts}",Delegavimas.Set).Swagger("","Pridėti deleguojamą asmenį").Produces(204).Errors(422,401,403,404).RequireRole();
app.MapDelete("/api/deleg/{gvts}/{user}",Delegavimas.Del).Swagger("","Trinti deleguojamą asmenį").Produces(204).Errors(422,401,403,404).RequireRole();

// Deklaracijos
app.MapGet("/api/deklaracijos/{gvts}",Deklaracija.GetAll).Swagger("","Gaunamos visos VGTS deklaracijos").Response<G9.Models.DeklaracijaGet>(200,401,403,404).RequireRole();
app.MapGet("/api/deklaracijos/{gvts}/{metai}",Deklaracija.GetYear).Swagger("","Gaunamos VGTS deklaracijos metams").Response<G9.Models.DeklaracijaGet>(200,401,403,404).RequireRole();

app.MapGet("/api/deklaracija/{deklaracija}",Deklaracija.GetOne).Swagger("","Deklaracijos duomenys").Response<G9.Models.DeklaracijaGet>(200,401,403,404).RequireRole();
app.MapPost("/api/deklaracija/{deklaracija}",Deklaracija.Set).Swagger("","Daklaracijos duomenų įvedimas").Response<G9.Models.DeklaracijaGet>(200,422,401,403,404).RequireRole();

// Deklaravimas
app.MapGet("/api/neatitiktys/{deklaracija}",Deklaravimas.Valid).Swagger("","Tikrinti ar galima delkaruoti").Response<G9.Models.Neatitiktys>(200,422,401,403,404).RequireRole();
app.MapPost("/api/neatitiktys/{deklaracija}",Deklaravimas.Update).Swagger("","Atnaujinti delkaracijos neatitiktis").Response<G9.Models.Neatitiktys>(200,422,401,403,404).RequireRole();
app.MapGet("/api/neatitiktys/{deklaracija}/{tipas}",Deklaravimas.GetOne).Swagger("","Gauti delkaracijos neatitiktis pagal tipą").Response<G9.Models.Neatitiktys>(200,422,401,403,404).RequireRole();
app.MapPost("/api/deklaruoti/{deklaracija}",Deklaravimas.Submit).Swagger("","Pateikti deklaraciją").Response<G9.Models.Deklaravimas>(200,422,401,403,404).RequireRole();
//app.MapPost("/api/deklaruoti/{deklaracija}/{tipas}",Deklaravimas.UpdateOne).Swagger("","Atnaujinti delkaracijos neatitiktis").Response<G9.Models.Deklaravimas>(200,422,401,403,404).RequireRole();



// Veiklos
app.MapGet("/api/veiklos",Veiklos.Get).Swagger("","Gaunamos prisijungusio vartotojo veiklos ir deklaruojami metai").Response<G9.Models.Veiklos>(200,401,403).RequireRole();

// Rodikliai
app.MapGet("/api/rodikliai",Rodikliai.List).Swagger("","Gaunamas pilnas rodiklių sąrašas").Response<G9.Models.RodikliuSarasas>(200).RequireLogin();

// Reikšmės
app.MapGet("/api/reiksmes/{deklaracija}",Reiksmes.Get).Swagger("","Gauti deklaruojamų rodiklių reikšmes").Response<G9.Models.ArrayModelA<G9.Models.RodiklioReiksme>>(200,401,403,404).RequireRole();
app.MapPost("/api/reiksmes/{deklaracija}",Reiksmes.Set).Swagger("","Įrašomos rodiklių reikšmės").Produces(204).Errors(422,401,403,404).RequireRole();
app.MapDelete("/api/reiksmes/{deklaracija}",Reiksmes.Del).Swagger("","Pašalinti rodiklių reikšmes deklaracijoje").Produces(204).Errors(422,401,403,404).RequireRole();


app.Run();
