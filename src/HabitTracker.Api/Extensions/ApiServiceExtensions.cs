namespace HabitTracker.Api.Extensions;

public static class ApiServiceExtensions
{
    public static WebApplicationBuilder AddApi(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddControllers(opt => opt.ReturnHttpNotAcceptable = true)
            .AddXmlDataContractSerializerFormatters();

        builder.Services.AddHealthChecks();
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();

        return builder;
    }
}
