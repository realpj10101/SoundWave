using api.Controllers.Helpers;
using api.DTOs;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Models.Helpers;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace api.Controllers;

public class PlaylistController(
    IPlaylistRepository playlistRepository,
    ITokenService tokenService
) : BaseApiController
{
    [HttpPost("add/{targetAudioName")]
    public async Task<ActionResult<Response>> Add(string targetAudioName, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("Ypu are not logged in. Please login again");

        PlaylistStatus pS = await playlistRepository.AddAsync(userId.Value, targetAudioName, cancellationToken);
        
        return pS.IsSuccess
            ?  Ok(new Response(Message: $"You add {targetAudioName} to your playlist successfully."))
            : pS.IsTargetAudioNotFound
            ? NotFound($"{targetAudioName} was not found")
            : pS.IsAlreadyAdded
            ? BadRequest($"{targetAudioName} already exists in the playlist")
            : BadRequest("Add to playlist failed try again or contact the administrator");
    }

    [HttpDelete("remove/{targetAudioName}")]
    public async Task<ActionResult<Response>> Remove(string targetAudioName, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);
        
        if (userId is null)
            return Unauthorized("Ypu are not logged in. Please login again");
        
        PlaylistStatus pS = await playlistRepository.RemoveAsync(userId.Value, targetAudioName, cancellationToken);
        
        return pS.IsSuccess
            ? Ok(new Response(Message: $"You removed {targetAudioName} form your playlist successfully."))
            : pS.IsTargetAudioNotFound
            ? NotFound($"{targetAudioName} was not found")
            : pS.IsAlreadyRemoved
            ? BadRequest("${targetAudioName} is not on the playlist")
            : BadRequest("Remove from playlist failed try again or contact the administrator");
    }
}