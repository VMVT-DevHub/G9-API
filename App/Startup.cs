using System.Text.Json;
using App.Auth;
using Microsoft.AspNetCore.Diagnostics;

/// <summary>Application initial startup class</summary>
public static class Startup {
	/// <summary></summary>
	public static string ConnStr { get; set; } = "";

	/// <summary>Build minimal API app</summary>
	/// <param name="args">primary execution arguments</param>
	/// <returns>WebApplication</returns>
	public static WebApplication Build(string[] args){
		var builder = WebApplication.CreateBuilder(args);
		builder.WebHost.UseKestrel(option => option.AddServerHeader = false);

		ConnStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

		#if DEBUG //Disable Swagger
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen( c => {
				c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo{Title="SwaggerAnnotation", Version="v1"});
				c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,"G9.xml"));
			});
			builder.Services.AddMvc().AddJsonOptions(opt => { 
				opt.JsonSerializerOptions.PropertyNamingPolicy = null; opt.JsonSerializerOptions.WriteIndented=false; 
				opt.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
				});
		#endif
		
		builder.Services.ConfigureHttpJsonOptions( a => { 
			a.SerializerOptions.PropertyNamingPolicy=null; 
			a.SerializerOptions.WriteIndented=false;
			a.SerializerOptions.Converters.Add(new CustomDateTimeConverter());
			a.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
		});
		

		var app = builder.Build();

		app.UseExceptionHandler(exh=>exh.Run(HandleError));

		#if DEBUG //Disable Swagger
			app.UseSwagger(); app.UseSwaggerUI();
		#endif
		return app;
	}


	private static async Task HandleError(HttpContext ctx){
		var rsp = ctx.Response;

		var ex = ctx.Features.Get<IExceptionHandlerFeature>();

		if(ex is not null && ex.Error is not null){
			await rsp.WriteAsync("Error...");
		}
	}

	/// <summary>Datos formatavimas</summary>
	public class CustomDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>{
		/// <summary></summary><param name="reader"></param><param name="typeToConvert"></param><param name="options"></param><returns></returns>
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => DateTime.TryParse(reader.GetString(),out var dt)?dt:default;
		/// <summary></summary><param name="writer"></param><param name="value"></param><param name="options"></param>
		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
	}

	/// <summary>Extension for route handler to add swagger info for dev environment</summary>
	/// <param name="rtx">Route handler</param>
	/// <param name="summary">API Summary</param>
	/// <param name="desc">API Description</param>
	/// <returns>Route handler</returns>
	public static RouteHandlerBuilder Swagger (this RouteHandlerBuilder rtx, string summary, string? desc = null){
		#if DEBUG //Disable swagger
			rtx.WithOpenApi(o => new(o){ Summary = summary, Description = desc });
		#endif
		return rtx;
	}


	/// <summary>Registruoti API atsakymo klaidas</summary>
	/// <param name="builder"></param><param name="err"></param><returns></returns>
	public static RouteHandlerBuilder Errors(this RouteHandlerBuilder builder,params int[] err){
		foreach (var i in err) {
			switch (i) {
				case 401: builder.Produces<E401>(401); break;
				case 403: builder.Produces<E403>(403); break;
				case 404: builder.Produces<E404>(404); break;
				case 422: builder.Produces<E422>(422); break;
			}
		}
		return builder;
	}

	/// <summary>Registruoti API atsakymo formatą</summary>
	/// <typeparam name="T">Formatas</typeparam><param name="builder"></param><returns></returns>
	public static RouteHandlerBuilder Response<T>(this RouteHandlerBuilder builder) => builder.Produces<T>(200);

	/// <summary>Registruoti API atsakymo formatą</summary>
	/// <typeparam name="T">Formatas</typeparam><param name="builder"></param>
	/// <param name="main">Pagrindinis atsakymo statusas</param>
	/// <param name="err">Klaidos kodai</param><returns></returns>
	public static RouteHandlerBuilder Response<T>(this RouteHandlerBuilder builder, int main=200, params int[] err) => builder.Produces<T>(main).Errors(err);

	/// <summary>Reikalauti vartotojo rolės</summary>
	/// <param name="builder"></param><returns></returns>
	public static RouteHandlerBuilder RequireRole(this RouteHandlerBuilder builder) => builder.AddEndpointFilter(Require.Role);

	/// <summary>Reikalauti vartotojo prisijungimo</summary>
	/// <param name="builder"></param><returns></returns>
	public static RouteHandlerBuilder RequireLogin(this RouteHandlerBuilder builder) => builder.AddEndpointFilter(Require.Login);

	/// <summary>Reikalauti API prisijungimo rakto</summary>
	/// <param name="builder"></param><returns></returns>
	public static RouteHandlerBuilder RequireAPIKey(this RouteHandlerBuilder builder) => builder.AddEndpointFilter(Require.APIKey);
}
