using api.DTOs.Account;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Settings;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<AppUser> _collection;
    private readonly ITokenService _tokenService;

    public UserRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings,
        ITokenService tokenService
    )
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        _tokenService = tokenService;
    }

    public async Task<UpdateResult?> UpdateUserAsync(UserUpdateDto userUpdateDto, string? hashedUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(hashedUserId)) return null;

        ObjectId? userId = await _tokenService.GetActualUserIdAsync(hashedUserId, cancellationToken);

        if (userId is null) return null;

        var updates = new List<UpdateDefinition<AppUser>>();

        if (!string.IsNullOrWhiteSpace(userUpdateDto.Bio))
            updates.Add(Builders<AppUser>.Update.Set(u => u.Bio, userUpdateDto.Bio.Trim().ToLower()));

        UpdateDefinition<AppUser>? updateDef = Builders<AppUser>.Update.Combine(updates);

        if (!updates.Any())
            return null;

        return await _collection.UpdateOneAsync<AppUser>(
            appUser => appUser.Id == userId,
            updateDef,
            null,
            cancellationToken
        );
    }
}