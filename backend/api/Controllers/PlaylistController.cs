using api.Controllers.Helpers;
using api.DTOs;
using api.DTOs.Track;
using api.Extensions;
using api.Helpers;
using api.Interfaces;
using api.Models;
using api.Models.Helpers;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using ZstdSharp.Unsafe;

namespace api.Controllers;

public class PlaylistController(
    IPlaylistRepository playlistRepository,
    ITokenService tokenService,
    ILikeRepository likeRepository
) : BaseApiController
{
    [HttpPost("add/{targetAudioName}")]
    public async Task<ActionResult<Response>> Add(string targetAudioName, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("Ypu are not logged in. Please login again");

        PlaylistStatus pS = await playlistRepository.AddAsync(userId.Value, targetAudioName, cancellationToken);

        return pS.IsSuccess
            ? Ok(new Response(Message: $"You add {targetAudioName} to your playlist successfully."))
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
            ? BadRequest($"{targetAudioName} is not on the playlist")
            : BadRequest("Remove from playlist failed try again or contact the administrator");
    }

    [HttpGet("get-playlists-count/{targetAudioName}")]
    public async Task<ActionResult<int>> GetLikesCount(string targetAudioName, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        int playlistCount = await playlistRepository.GetPlaylistsCount(targetAudioName, cancellationToken);

        return playlistCount;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AudioFileResponse>>> GetAll([FromQuery] PlaylistParams playlistParams, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again.");

        playlistParams.UserId = userId;

        PagedList<AudioFile> pagedAudioFiles = await playlistRepository.GetAllAsync(playlistParams, cancellationToken);

        if (pagedAudioFiles.Count == 0) return NoContent();

        Response.AddPaginationHeader(new(
            pagedAudioFiles.CurrentPage,
            pagedAudioFiles.PageSize,
            pagedAudioFiles.TotalItems,
            pagedAudioFiles.TotalPages
        ));

        List<AudioFileResponse> audioFileResponses = [];

        bool isLiking;
        bool isAdding;
        foreach (AudioFile audioFile in pagedAudioFiles)
        {
            isLiking = await likeRepository.CheckIsLikingAsync(userId.Value, audioFile, cancellationToken);

            isAdding = await playlistRepository.CheckIsAddingAsync(userId.Value, audioFile, cancellationToken);

            audioFileResponses.Add(Mappers.ConvertAudioFileToAudioFileResponse(audioFile, isLiking, isAdding));
        }

        return audioFileResponses;
    }
}