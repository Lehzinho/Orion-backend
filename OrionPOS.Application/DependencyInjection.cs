using Microsoft.Extensions.DependencyInjection;
using OrionPOS.Application.Auth;
using OrionPOS.Application.Catalogo;

namespace OrionPOS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GetCardapioUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<GetMeUseCase>();
        return services;
    }
}
