using api.DTOs.Account;
using MongoDB.Driver;

namespace api.Interfaces;

public interface IUserRepository
{
    public Task<UpdateResult?> UpdateUserAsync(UserUpdateDto userUpdateDto, string? hashedUserId, CancellationToken cancellationToken);    
}