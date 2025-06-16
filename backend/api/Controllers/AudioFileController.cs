using api.Controllers.Helpers;
using api.DTOs;
using api.DTOs.Account;
using api.DTOs.Helpers;
using api.DTOs.Track;
using api.Extensions;
using api.Helpers;
using api.Interfaces;
using api.Models;
using api.Models.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace api.Controllers;

[Authorize]
public class AudioFileController(
    IAudioFileRepository _audioFileRepository, ITokenService _tokenService, ILikeRepository _likeRepository) : BaseApiController
{
    [HttpPost("upload")]
    public async Task<ActionResult<Response>> Upload(
     [FromForm] IFormFile file, CancellationToken cancellationToken
    )
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        if (file == null || file.Length == 0)
            return BadRequest("File is empty");

        if (file.Length > 40_000_000)
            return BadRequest("File is more than 40MB");

        if (!file.FileName.EndsWith(".mp3"))
            return BadRequest("Only mp3 files are allowed.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        CreateAudioFile createAudioFile = new(
            FileName: file.FileName,
            FileData: ms.ToArray()
        );

        OperationResult<AudioFile> opResult = await _audioFileRepository.UploadAsync(createAudioFile, userId, cancellationToken);

        return opResult.IsSuccess
            ? Ok(new Response(Message: "File uploaded successfully."))
            : opResult.Error?.Code switch
            {
                ErrorCode.IsNotFound => BadRequest(opResult.Error.Message),
                _ => BadRequest("Operation failed!. Try again or contact administrator")
            };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AudioFileResponse>>> GetAll([FromQuery] AudioFileParams audioFileParams, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        PagedList<AudioFile>? pagedAudioFiles = await _audioFileRepository.GetAllAsync(audioFileParams, cancellationToken);

        if (pagedAudioFiles.Count == 0)
            return NoContent();

        PaginationHeader paginationHeader = new(
            CurrentPage: pagedAudioFiles.CurrentPage,
            ItemsPerPage: pagedAudioFiles.PageSize,
            TotalItems: pagedAudioFiles.TotalItems,
            TotalPages: pagedAudioFiles.TotalPages
        );

        Response.AddPaginationHeader(paginationHeader);

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