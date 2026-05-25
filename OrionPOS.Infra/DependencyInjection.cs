using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrionPOS.Application.Auth;
using OrionPOS.Application.Catalogo;
using OrionPOS.Infra.Persistence;
using OrionPOS.Infra.Repositories;
using OrionPOS.Infra.Security;

namespace OrionPOS.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=orionpos;Username=postgres;Password=postgres";

        services.AddDbContext<OrionPosDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
