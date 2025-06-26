using api.DTOs;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Settings;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly IMongoCollection<AppUser>? _collection;
    private readonly ITokenService _tokenService;

    public MemberRepository(IMongoClient client, IMyMongoDbSettings dbSettings,
        ITokenService tokenService)
    {
        IMongoDatabase? database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        _tokenService = tokenService;
    }

    public async Task<MemberDto?> GetByUserNameAsync(string userName, string userIdHashed, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(userIdHashed, cancellationToken);

        if (userId is null) return null;

        AppUser appUser = await _collection.Find(appUser =>
            appUser.NormalizedUserName == userName.ToUpper()).FirstOrDefaultAsync(cancellationToken);

        if (appUser is null) return null;

        return appUser is not null
            ? Mappers.ConvertAppUserToMemberDto(appUser)
            : null;
    }
}