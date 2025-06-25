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

namespace api.Controllers;

public class LikeController(
    ILikeRepository _likeRepository, ITokenService _tokenService
) : BaseApiController
{
    [HttpPost("add/{targetAudioName}")]
    public async Task<ActionResult<Response>> Create(string targetAudioName, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        LikeStatus lS = await _likeRepository.CreateAsync(userId.Value, targetAudioName, cancellationToken);

        return lS.IsSuccess
            ? Ok(new Response(Message: $"You liked {targetAudioName} successfully"))
            : lS.IsTargetAudioNotFound
            ? NotFound($"{targetAudioName} was not found.")
            : lS.IsAlreadyLiked
            ? BadRequest($"{targetAudioName} is already liked.")
            : BadRequest("Liking failed. Please try again or contact the administrator.");
    }

    [HttpDelete("remove/{targetAudioName}")]
    public async Task<ActionResult<Response>> Delete(string targetAudioName, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        LikeStatus lS = await _likeRepository.DeleteAsync(userId.Value, targetAudioName, cancellationToken);

        return lS.IsSuccess
            ? Ok(new Response(Message: $"You dislike {targetAudioName} successfully"))
            : lS.IsTargetAudioNotFound
            ? NotFound($"{targetAudioName} was not found.")
            : lS.IsAlreadyDisLiked
            ? BadRequest($"{targetAudioName} is already disliked.")
            : BadRequest("Disliking failed. Please try again or contact the administrator.");
    }

    [HttpGet("get-likes-count/{targetAudioName}")]
    public async Task<ActionResult<int>> GetLikesCount(string targetAudioName, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        int likeCount = await _likeRepository.GetLikesCount(targetAudioName, cancellationToken);

        return likeCount;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AudioFileResponse>>> GetAll([FromQuery] LikeParams likeParams, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        likeParams.UserId = userId;

        PagedList<AudioFile> pagedAudioFiles = await _likeRepository.GetAllAsync(likeParams, cancellationToken);

        if (pagedAudioFiles.Count == 0) return NoContent();

        Response.AddPaginationHeader(new(
            pagedAudioFiles.CurrentPage,
            pagedAudioFiles.PageSize,
            pagedAudioFiles.TotalItems,
            pagedAudioFiles.TotalPages
        ));

        List<AudioFileResponse> audioFileResponses = [];

        bool isLiking;
        foreach (AudioFile audioFile in pagedAudioFiles)
        {
            isLiking = await _likeRepository.CheckIsLikingAsync(userId.Value, audioFile, cancellationToken);

            audioFileResponses.Add(Mappers.ConvertAudioFileToAudioFileResponse(audioFile, isLiking));
        }

        return audioFileResponses;
    }
}