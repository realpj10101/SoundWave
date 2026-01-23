using api.Controllers.Helpers;
using api.DTOs;
using api.DTOs.Account;
using api.DTOs.Helpers;
using api.Extensions;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace api.Controllers;

public class CommentController(
    ICommentRepository _commentRepository, ITokenService _tokenService) : BaseApiController
{
    [HttpPost("add/{targetAudioId}")]
    public async Task<ActionResult<CommentResponse>> Create(string targetAudioId, CreateCommentDto content,
        CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        if (!ObjectId.TryParse(targetAudioId, out var audioId))
            return BadRequest("Enter valid audio information");

        OperationResult<CommentResponse> opResult =
            await _commentRepository.CreateAsync(userId.Value, audioId, content.Content, cancellationToken);

        return opResult.IsSuccess
            ? Ok(opResult.Result)
            : opResult.Error?.Code switch
            {
                ErrorCode.IsNotFound => BadRequest(opResult.Error.Message),
                _ => BadRequest("Operation failed! Try again or contact support.")
            };
    }

    // [AllowAnonymous]
    [HttpGet("get-comments/{targetAudioId}")]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetComments(string targetAudioId,
        CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(targetAudioId, out var audioId))
            return BadRequest("Enter valid audio information");

        OperationResult<IEnumerable<CommentResponse>> opResult = await _commentRepository.GetAllAudioCommentsAsync(audioId, cancellationToken);

        return opResult.IsSuccess
            ? Ok(opResult.Result)
            : NoContent();
    }
}