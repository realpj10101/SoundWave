using api.Interfaces;
using api.Repositories;
using api.Services;
using image_processing.Interfaces;
using image_processing.Services;

namespace api.Extensions;

public static class RepositoryServiceExtensions
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        #region Player
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAudioFileRepository, AudioFileRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IPhotoModifySaveService, PhotoModifySaveService>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IAudioService, AudioService>();
        #endregion

        return services;
    }
}
