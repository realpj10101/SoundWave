using api.Controllers.Helpers;
using api.DTOs;
using api.Extensions;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Authorize]
public class MemberController(IMemberRepository _memberRepository) : BaseApiController
{
    [HttpGet("get-by-username/{userName}")]
    public async Task<ActionResult<MemberDto>> GetByUserName(string userName, CancellationToken cancellationToken)
    {
        string? userIdHashed = User.GetHashedUserId();

        if (userIdHashed is null)
            return Unauthorized("You are not loggged in. Please login again");

        MemberDto? memberDto = await _memberRepository.GetByUserNameAsync(userName, userIdHashed, cancellationToken);

        return memberDto is null
            ? BadRequest("Target member not found")
            : memberDto;
    }
}