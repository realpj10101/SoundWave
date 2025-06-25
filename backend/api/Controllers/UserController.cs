using api.Controllers.Helpers;
using api.DTOs.Account;
using api.Extensions;
using api.Extensions.Validations;
using api.Interfaces;
using api.Models;
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

    [HttpPost("add-photo")]
    public async Task<ActionResult<Photo>> AddPhoto(
        [AllowedFileExtensions, FileSize(250_000, 4_000_000)]
        IFormFile file, CancellationToken cancellationToken
    )
    {
        if (file is null) return BadRequest("No file is selected with this request");

        Photo? photo = await _userRepository.UploadPhotoAsync(file, User.GetHashedUserId(), cancellationToken);

        return photo is null ? BadRequest("Add photo failed. See logger") : photo;
    }
}