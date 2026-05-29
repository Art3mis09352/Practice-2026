using Microsoft.Extensions.DependencyInjection;

namespace Application.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationFeatures(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
