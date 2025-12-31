using api.DTOs.Account;
using api.DTOs.Helpers;
using api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Interfaces;

public interface IUserRepository
{
    public Task<UpdateResult?> UpdateUserAsync(UserUpdateDto userUpdateDto, string? hashedUserId, CancellationToken cancellationToken);
    public Task<AppUser?> GetByIdAsync(ObjectId userId, CancellationToken cancellationToken);
    public Task<OperationResult<MainPhoto>> UploadPhotoAsync(IFormFile file, ObjectId userId, CancellationToken cancellationToken);
    // public Task<UpdateResult?> SetMainPhotoAsync(string hashedUserId, string photoUrlIn, CancellationToken cancellationToken);
    // public Task<UpdateResult?> DeletePhotoAsync(string hashedUserId, string? urlIn, CancellationToken cancellationToken);
}