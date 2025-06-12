using api.Interfaces;
using api.Services;

namespace api.Extensions;

public static class RepositoryServiceExtensions
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        #region Player
        services.AddScoped<ITokenService, TokenService>();
        #endregion

        return services;
    }
}
