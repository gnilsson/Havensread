using Havensread.Api.Endpoints;

namespace Havensread.Api.ServiceConfiguration;

public static class ApplicationInitializationExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        services.AddScoped<GetBooks.Endpoint>();
        return services;
    }
}
