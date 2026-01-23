using api.DTOs;
using api.DTOs.Helpers;
using MongoDB.Bson;

namespace api.Interfaces;

public interface IMemberRepository
{
    public Task<MemberDto?> GetByUserNameAsync(string userName, string userIdHashed, CancellationToken cancellationToken);
    public Task<OperationResult<string>> GetUserNameByObjectIdAsync(ObjectId userId, CancellationToken cancellationToken);
}