
using System.Data.Common;
using App.Auth;

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
			builder.Services.AddMvc().AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null);
		#endif
		
		builder.Services.ConfigureHttpJsonOptions( a => { a.SerializerOptions.PropertyNamingPolicy=null; });

		var app = builder.Build();

		#if DEBUG //Disable Swagger
			app.UseSwagger(); app.UseSwaggerUI();
		#endif
		return app;
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
}