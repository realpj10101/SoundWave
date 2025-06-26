using api.DTOs;
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
    private readonly ILogger<UserRepository> _logger;
    private readonly IPhotoService _photoService;

    public UserRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings,
        ITokenService tokenService, ILogger<UserRepository> logger,
        IPhotoService photoService
    )
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        _tokenService = tokenService;
        _photoService = photoService;
        _logger = logger;
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

    public async Task<AppUser?> GetByIdAsync(ObjectId userId, CancellationToken cancellationToken)
    {
        AppUser? appUser = await _collection.Find<AppUser>(user
            => user.Id == userId).SingleOrDefaultAsync(cancellationToken);

        if (appUser is null) return null;

        return appUser;
    }

    public async Task<Photo?> UploadPhotoAsync(IFormFile file, string? hashedUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(hashedUserId)) return null;

        ObjectId? userId = await _tokenService.GetActualUserIdAsync(hashedUserId, cancellationToken);

        if (userId is null) return null;

        AppUser? appUser = await GetByIdAsync(userId.Value, cancellationToken);
        if (appUser is null)
        {
            _logger.LogError("appUser is Null / not found");
            return null;
        }

        string[]? imageUrls = await _photoService.AddPhotoToDiskAsync(file, userId.Value);

        if (imageUrls is not null)
        {
            Photo photo;

            photo = appUser.Photos.Count == 0
                ? Mappers.ConvertPhotoUrlsToPhoto(imageUrls, isMain: true)
                : Mappers.ConvertPhotoUrlsToPhoto(imageUrls, isMain: false);

            appUser.Photos.Add(photo);

            // photo = string.IsNullOrWhiteSpace(appUser.Photo.Url_enlarged)
            //     ? Mappers.ConvertPhotoUrlsToPhoto(imageUrls, isMain: true)
            //     : Mappers.ConvertPhotoUrlsToPhoto(imageUrls, isMain: true);

            // appUser.Photo = photo;

            var updatedUser = Builders<AppUser>.Update
                .Set(doc => doc.Photos, appUser.Photos);

            UpdateResult result = await _collection.UpdateOneAsync<AppUser>(doc => doc.Id == userId, updatedUser, null, cancellationToken);

            return result.ModifiedCount == 1 ? photo : null;
        }

        _logger.LogError("PhotoService saving photo to disk failed.");
        return null;
    }
}