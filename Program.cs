using G9;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/",()=>"Ok");

app.MapGet("/test", async (HttpContext ctx) =>{
    //var streamResponse = await httpClient.GetStreamAsync("posts");
    //return Results.Stream(streamResponse, "application/json");
	await ctx.Response.WriteAsync("labas 1");
}).WithDescription("Some Method Description").WithOpenApi(); //.ExcludeFromDescription();


app.MapGet("/test2", G9.APIS.GoodMinimalApi).WithOpenApi(o => new(o){	
    Summary = "Reads text from PDF",
    Description = "This API returns all text in PDF document"
}).Produces<List<TestItem>>(200); //.ExcludeFromDescription();


app.Run();
