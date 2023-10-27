
/// <summary>Application initial startup class</summary>
public static class Startup {
	/// <summary>Build minimal API app</summary>
	/// <param name="args">primary execution arguments</param>
	/// <returns>WebApplication</returns>
	public static WebApplication Build(string[] args){
		var builder = WebApplication.CreateBuilder(args);

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
}