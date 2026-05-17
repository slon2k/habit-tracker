using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi;

namespace HabitTracker.Api.Startup;

public static class ApiStartup
{
    public static WebApplicationBuilder AddApi(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddControllers(opt => opt.ReturnHttpNotAcceptable = true)
            .AddXmlDataContractSerializerFormatters();

        builder.Services.Configure<MvcOptions>(options =>
        {
            var jsonOutputFormatter = options.OutputFormatters
                .OfType<SystemTextJsonOutputFormatter>()
                .FirstOrDefault();

            if (jsonOutputFormatter is not null && !jsonOutputFormatter.SupportedMediaTypes.Contains("application/hal+json"))
            {
                jsonOutputFormatter.SupportedMediaTypes.Add("application/hal+json");
            }
        });

        builder.Services.AddHealthChecks();
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Add JWT Bearer security definition
            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                Description = "Enter your JWT token"
            });

            // Apply security requirement to all endpoints
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });

        return builder;
    }
}
