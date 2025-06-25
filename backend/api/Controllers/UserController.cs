using api.Controllers.Helpers;
using api.DTOs.Account;
using api.Extensions;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Controllers;

[Authorize]
public class UserController(IUserRepository _userRepository, ITokenService _tokenService) : BaseApiController
{
    [HttpPut]
    public async Task<ActionResult> UpdateUser(UserUpdateDto userUpdateDto, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        UpdateResult? updateResult = await _userRepository.UpdateUserAsync(userUpdateDto, User.GetHashedUserId(), cancellationToken);

        return updateResult is null || !updateResult.IsModifiedCountAvailable
            ? BadRequest("upadte failed! Try again later")
            : Ok(new { message = "User has been updated successfully" });
    }
}