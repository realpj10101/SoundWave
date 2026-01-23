using api.Controllers.Helpers;
using api.DTOs.Account;
using api.DTOs.Helpers;
using api.Extensions;
using api.Extensions.Validations;
using api.Interfaces;
using api.Models;
using api.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
    public async Task<ActionResult<MainPhoto>> AddPhoto(
        [AllowedFileExtensions, FileSize(250_000, 4_000_000)]
        IFormFile file, CancellationToken cancellationToken
    )
    {
        if (file is null) return BadRequest("No file is selected with this request");

        string? hashedUserId = User.GetHashedUserId();

        if (hashedUserId is null)
            return Unauthorized("You are not logged in. Please login again.");

        ObjectId? userId = await _tokenService.GetActualUserIdAsync(hashedUserId, cancellationToken);

        OperationResult<MainPhoto> opResult = await _userRepository.UploadPhotoAsync(file, userId.Value, cancellationToken);

        return opResult.IsSuccess
        ? opResult.Result
        : opResult.Error?.Code switch
        {
            ErrorCode.IsNotFound => BadRequest(opResult.Error.Message),
            ErrorCode.IsUpdateFailed => BadRequest(opResult.Error.Message),
            ErrorCode.IsAddPhotoFailed => BadRequest(opResult.Error.Message),
            _ => BadRequest("Operation failed! Try again or contact support.")
        };
    }

    // [HttpPut("set-main-photo")]
    // public async Task<ActionResult> SetMainPhoto(string photoUrlIn, CancellationToken cancellationToken)
    // {
    //     string? hashedUserId = User.GetHashedUserId();

    //     if (string.IsNullOrEmpty(hashedUserId))
    //         return Unauthorized("You are not logged in. Please login again");

    //     UpdateResult? updateResult =
    //         await _userRepository.SetMainPhotoAsync(hashedUserId, photoUrlIn, cancellationToken);

    //     return updateResult is null || !updateResult.IsModifiedCountAvailable
    //         ? BadRequest("Update failed. Try agian later")
    //         : Ok(new { message = "User has been updated successfully." });
    // }

    // [HttpPut("delete-photo")]
    // public async Task<ActionResult> DeletePhoto(string photoUrlIn, CancellationToken cancellationToken)
    // {
    //     string? hashedUserId = User.GetHashedUserId();

    //     if (string.IsNullOrEmpty(hashedUserId))
    //         return Unauthorized("You are not logged in. Please login again.");

    //     UpdateResult? updateResult =
    //         await _userRepository.DeletePhotoAsync(hashedUserId, photoUrlIn, cancellationToken);

    //     return updateResult is null || !updateResult.IsModifiedCountAvailable
    //         ? BadRequest("Photo delection failed. Try again or conctact the support")
    //         : Ok(new { message = "Photo deleted successfully." });
    // }
}