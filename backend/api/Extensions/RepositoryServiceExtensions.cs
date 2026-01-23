using api.Interfaces;
using api.Repositories;
using api.Services;
using Image_Processing_WwwRoot.Interfaces;
using Image_Processing_WwwRoot.Services;

namespace api.Extensions;

public static class RepositoryServiceExtensions
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        #region 
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAudioFileRepository, AudioFileRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IPhotoModifySaveService, PhotoModifySaveService>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IAudioService, AudioService>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();
        services.AddScoped<IAiNluService, AiNluService>();
        services.AddScoped<IAiRecommendService, AiRecommendService>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        #endregion

        return services;
    }
}
