using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace HabitTracker.Api.Extensions;

public static class ApiServiceExtensions
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

        return builder;
    }
}
