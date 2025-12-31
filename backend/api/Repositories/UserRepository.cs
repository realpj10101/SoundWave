using api.DTOs;
using api.DTOs.Account;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

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

    public async Task<MainPhoto?> UploadPhotoAsync(IFormFile file, string? hashedUserId, CancellationToken cancellationToken)
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

        string[]? imageUrls = await _photoService.AddMainPhotoToDiskAsync(file, appUser.Photo, userId.Value);

        if (imageUrls is not null)
        {
            MainPhoto photo;
            
            photo = Mappers.ConvertMainPhotoUrlsToPhoto(imageUrls);

            var updatedUser = Builders<AppUser>.Update
                .Set(doc => doc.Photo, appUser.Photo);

            UpdateResult result = await _collection.UpdateOneAsync<AppUser>(doc => doc.Id == userId, updatedUser, null, cancellationToken);

            return result.ModifiedCount == 1 ? photo : null;
        }

        _logger.LogError("PhotoService saving photo to disk failed.");
        return null;
    }

    // public async Task<UpdateResult?> SetMainPhotoAsync(string hashedUserId, string photoUrlIn, CancellationToken cancellationToken)
    // {
    //     ObjectId? userId = await _tokenService.GetActualUserIdAsync(hashedUserId, cancellationToken);

    //     if (userId is null) return null;

    //     #region Unset the previous main photo 

    //     FilterDefinition<AppUser>? filterOld = Builders<AppUser>.Filter
    //         .Where(appUser =>
    //             appUser.Id == userId && appUser.Photo.Any<Photo>(photo => photo.IsMain == true));

    //     UpdateDefinition<AppUser>? updateOld = Builders<AppUser>.Update
    //         .Set(appUser => appUser.Photo.FirstMatchingElement().IsMain, false);

    //     await _collection.UpdateOneAsync(filterOld, updateOld, null, cancellationToken);

    //     #endregion

    //     #region set the new main photo

    //     FilterDefinition<AppUser>? filterNew = Builders<AppUser>.Filter
    //         .Where(appUser =>
    //             appUser.Id == userId && appUser.Photo.Any<Photo>(photo => photo.Url_165 == photoUrlIn));

    //     UpdateDefinition<AppUser> updateNew = Builders<AppUser>.Update
    //         .Set(appUser => appUser.Photo.FirstMatchingElement().IsMain, true);

    //     return await _collection.UpdateOneAsync(filterNew, updateNew, null, cancellationToken);

    //     #endregion
    // }

    // public async Task<UpdateResult?> DeletePhotoAsync(string hashedUserId, string? urlIn, CancellationToken cancellationToken)
    // {
    //     if (string.IsNullOrEmpty(urlIn)) return null;

    //     ObjectId? userId = await _tokenService.GetActualUserIdAsync(hashedUserId, cancellationToken);

    //     if (userId is null) return null;

    //     Photo photo = await _collection.AsQueryable()
    //         .Where(appUser => appUser.Id == userId) // filter by user email
    //         .SelectMany(appUser => appUser.Photo) // flatten
    //         .Where(photo => photo.Url_165 == urlIn)
    //         .FirstOrDefaultAsync(cancellationToken);

    //     if (photo is null) return null;

    //     if (photo.IsMain) return null;

    //     bool isDelereSuccess = await _photoService.DeletePhotoFromDisk(photo);
    //     if (!isDelereSuccess)
    //     {
    //         _logger.LogError("Delete Photo from disk failed");

    //         return null;
    //     }

    //     var update = Builders<AppUser>.Update
    //         .PullFilter(appUser => appUser.Photo, photo => photo.Url_165 == urlIn);

    //     return await _collection.UpdateOneAsync<AppUser>(appUser => appUser.Id == userId, update, null, cancellationToken);
    // }
}