using Exchange.Api.Authentication;
using Exchange.Api.Options;
using Exchange.Api.Services;
using Exchange.Configuration;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

namespace Exchange.Api
{
    public static class ApiApplication
    {
        public static WebApplication Build(string[] args, Action<WebApplicationBuilder>? customize = null)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ApplicationName = typeof(ApiApplication).Assembly.FullName
            });
            builder.Services.AddControllers().AddApplicationPart(typeof(ApiApplication).Assembly);
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Exchange Admin API", Version = "v1" });
                options.AddSecurityDefinition(ApiKeyAuthenticationHandler.SchemeName, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "X-Api-Key",
                    Description = "Enter the configured API key."
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(ApiKeyAuthenticationHandler.SchemeName, document)] = []
                });
            });
            builder.Services.Configure<ApiBehaviorOptions>(o => o.InvalidModelStateResponseFactory = _ =>
                new BadRequestObjectResult(new ProblemDetails { Status = 400, Title = "Invalid request", Extensions = { ["code"] = "invalid_request" } }));
            builder.Services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, _ => { });
            builder.Services.AddAuthorization();
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IConfiguration>().GetSection("AdminApi").Get<ApiOptions>() ?? new ApiOptions());
            builder.Services.AddSingleton<Func<IReadOnlyDictionary<string, string>>>(_ => ExchangeConfigurationLoader.CaptureEnvironment);
            builder.Services.AddSingleton<UtilityLaunchPreparation>();
            builder.Services.AddSingleton<IUtilityProcessLauncher, UtilityProcessLauncher>();
            customize?.Invoke(builder);
            var app = builder.Build();
            try
            {
                if (string.IsNullOrWhiteSpace(app.Services.GetRequiredService<ApiOptions>().ApiKey))
                {
                    throw new InvalidOperationException("AdminApi:ApiKey is required.");
                }
                app.Services.GetRequiredService<UtilityLaunchPreparation>().Prepare();
            }
            catch
            {
                app.DisposeAsync().AsTask().GetAwaiter().GetResult();
                throw new InvalidOperationException("Admin API configuration is invalid. Check the API key, utility executable and required utility configuration files.");
            }
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("./swagger/v1/swagger.json", "Exchange Admin API v1");
            });
            app.MapControllers();
            return app;
        }
    }
}