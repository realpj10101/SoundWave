using api.Controllers.Helpers;
using api.DTOs;
using api.DTOs.Track;
using api.Extensions;
using api.Interfaces;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace api.Controllers;

public class AiController(IAiRecommendService _aiRecommendService, ITokenService _tokenService) : BaseApiController
{
    [HttpPost("recommend")]
    public async Task<ActionResult<IEnumerable<AudioFileResponse>>> Recommend(AskDto askDto, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        AiRecommendResponse result = await _aiRecommendService.RecommendAsync(askDto.Prompt, userId.Value, cancellationToken);

        if (result.Items.Count == 0)
            return NoContent();

        return Ok(result);
    }
}