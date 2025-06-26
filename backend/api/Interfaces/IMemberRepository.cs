using api.DTOs;

namespace api.Interfaces;

public interface IMemberRepository
{
    public Task<MemberDto?> GetByUserNameAsync(string userName, string userIdHashed, CancellationToken cancellationToken);
}