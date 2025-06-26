using api.DTOs.Account;
using api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Interfaces;

public interface IUserRepository
{
    public Task<UpdateResult?> UpdateUserAsync(UserUpdateDto userUpdateDto, string? hashedUserId, CancellationToken cancellationToken);
    public Task<AppUser?> GetByIdAsync(ObjectId userId, CancellationToken cancellationToken);
    public Task<Photo?> UploadPhotoAsync(IFormFile file, string? hashedUserId, CancellationToken cancellationToken);
    public Task<UpdateResult?> SetMainPhotoAsync(string hashedUserId, string photoUrlIn, CancellationToken cancellationToken);
    public Task<UpdateResult?> DeletePhotoAsync(string hashedUserId, string? urlIn, CancellationToken cancellationToken);
}