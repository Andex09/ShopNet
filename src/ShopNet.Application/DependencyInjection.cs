using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopNet.Application.Features.Orders;

namespace ShopNet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // One class per use case; every concrete "*Handler" is registered as itself.
        var handlers = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("Handler", StringComparison.Ordinal));
        foreach (var handler in handlers)
        {
            services.AddScoped(handler);
        }

        // API messages are always English, regardless of the server culture; the client shows its own Spanish texts.
        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.InvariantCulture;
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddOptions<OrderOptions>()
            .Bind(configuration.GetSection(OrderOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
