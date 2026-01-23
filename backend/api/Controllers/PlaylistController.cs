using api.Controllers.Helpers;
using api.DTOs;
using api.DTOs.Helpers;
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
    ILikeRepository likeRepository,
    IMemberRepository _memberRepository
) : BaseApiController
{
    [HttpPost("add/{targetAudioId}")]
    public async Task<ActionResult<Response>> Add(string targetAudioId, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("Ypu are not logged in. Please login again");

        if (!ObjectId.TryParse(targetAudioId, out var audioId))
            return BadRequest("Enter valid audio information");
        
        PlaylistStatus pS = await playlistRepository.AddAsync(userId.Value, audioId, cancellationToken);

        return pS.IsSuccess
            ? Ok(new Response(Message: $"You add audio to your playlist successfully."))
            : pS.IsTargetAudioNotFound
            ? NotFound($"Target audio was not found")
            : pS.IsAlreadyAdded
            ? BadRequest($"Target audio already exists in the playlist")
            : BadRequest("Add to playlist failed try again or contact the administrator");
    }

    [HttpDelete("remove/{targetAudioId}")]
    public async Task<ActionResult<Response>> Remove(string targetAudioId, CancellationToken cancellationToken)
    {
        ObjectId? userId = await tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("Ypu are not logged in. Please login again");
        
        if (!ObjectId.TryParse(targetAudioId, out var audioId))
            return BadRequest("Enter valid audio information");
        
        PlaylistStatus pS = await playlistRepository.RemoveAsync(userId.Value, audioId, cancellationToken);

        return pS.IsSuccess
            ? Ok(new Response(Message: $"You removed audio form your playlist successfully."))
            : pS.IsTargetAudioNotFound
            ? NotFound($"Target audio was not found")
            : pS.IsAlreadyRemoved
            ? BadRequest($"Target audio is not on the playlist")
            : BadRequest("Remove from playlist failed try again or contact the administrator");
    }

    [HttpGet("get-adders-count/{targetAudioName}")]
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
            
            OperationResult<string> opResult =
                await _memberRepository.GetUserNameByObjectIdAsync(audioFile.UploaderId, cancellationToken);

            audioFileResponses.Add(Mappers.ConvertAudioFileToAudioFileResponse(audioFile, opResult.Result, isLiking, isAdding));
        }

        return audioFileResponses;
    }
}