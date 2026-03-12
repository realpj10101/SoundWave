using api.Controllers.Helpers;
using api.DTOs;
using api.DTOs.Account;
using api.DTOs.Helpers;
using api.DTOs.Track;
using api.Extensions;
using api.Extensions.Validations;
using api.Helpers;
using api.Interfaces;
using api.Models;
using api.Models.Helpers;
using api.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace api.Controllers;

[Authorize]
public class AudioFileController(
    IAudioFileRepository _audioFileRepository,
    ITokenService _tokenService,
    ILikeRepository _likeRepository,
    IPlaylistRepository _playlistRepository,
    IMemberRepository _memberRepository
) : BaseApiController
{
    [HttpPost("upload")]
    public async Task<ActionResult<Response>> Upload(
        [FromForm, AllowedFileExtensions, FileSize(250_000, 40_000_000)]
        CreateAudioFile file, CancellationToken cancellationToken
    )
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        OperationResult<AudioFile> opResult = await _audioFileRepository.UploadAsync(file, userId, cancellationToken);

        return opResult.IsSuccess
            ? Ok(new Response(Message: "File uploaded successfully."))
            : opResult.Error?.Code switch
            {
                ErrorCode.IsFailed => BadRequest(opResult.Error.Message),
                ErrorCode.IsAlreadyExist => BadRequest(opResult.Error.Message),
                ErrorCode.IsNotFound => BadRequest(opResult.Error.Message),
                ErrorCode.SaveAudioFailed => BadRequest(opResult.Error.Message),
                ErrorCode.SavePhotoFailed => BadRequest(opResult.Error.Message),
                _ => BadRequest("Operation failed!. Try again or contact administrator")
            };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AudioFileResponse>>> GetAll([FromQuery] AudioFileParams audioFileParams,
        CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        PagedList<AudioFile>? pagedAudioFiles =
            await _audioFileRepository.GetAllAsync(audioFileParams, cancellationToken);

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
        bool isAdding;
        foreach (AudioFile audioFile in pagedAudioFiles)
        {
            isLiking = await _likeRepository.CheckIsLikingAsync(userId.Value, audioFile, cancellationToken);
            isAdding = await _playlistRepository.CheckIsAddingAsync(userId.Value, audioFile, cancellationToken);

            OperationResult<string> opResult =
                await _memberRepository.GetUserNameByObjectIdAsync(audioFile.UploaderId, cancellationToken);

            audioFileResponses.Add(
                Mappers.ConvertAudioFileToAudioFileResponse(audioFile, opResult.Result, isLiking, isAdding));
        }

        return audioFileResponses;
    }

    [HttpGet("get-user-audios")]
    public async Task<ActionResult<IEnumerable<AudioFileResponse>>> GetUserAudios(
        [FromQuery] AudioFileParams audioFileParams, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        PagedList<AudioFile>? pagedAudioFiles =
            await _audioFileRepository.GetUserAudioFiles(userId, audioFileParams, cancellationToken);
        
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
        bool isAdding;
        foreach (AudioFile audioFile in pagedAudioFiles)
        {
            isLiking = await _likeRepository.CheckIsLikingAsync(userId.Value, audioFile, cancellationToken);
            isAdding = await _playlistRepository.CheckIsAddingAsync(userId.Value, audioFile, cancellationToken);

            OperationResult<string> opResult =
                await _memberRepository.GetUserNameByObjectIdAsync(audioFile.UploaderId, cancellationToken);

            audioFileResponses.Add(
                Mappers.ConvertAudioFileToAudioFileResponse(audioFile, opResult.Result, isLiking, isAdding));
        }

        return audioFileResponses;
    }

    [HttpGet("stream/{audioId}")]
    public async Task<ActionResult> StreamAudio(ObjectId audioId, CancellationToken cancellationToken)
    {
        string? hashedUserId = User.GetHashedUserId();

        if (hashedUserId is null)
            return Unauthorized("You are not logged in. Please login again.");

        ObjectId? userId = await _tokenService.GetActualUserIdAsync(hashedUserId, cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again.");

        OperationResult<AudioFile> opResult =
            await _audioFileRepository.GetAudioFileByIdAsync(audioId, cancellationToken);

        if (!opResult.IsSuccess)
        {
            return opResult.Error?.Code switch
            {
                ErrorCode.AudioNotFound => BadRequest(opResult.Error.Message),
                _ => BadRequest("Operation failed!. Try again or contact administrator")
            };
        }

        var path = Path.Combine("wwwroot", opResult.Result.FilePath);

        if (!System.IO.File.Exists(path))
            return NotFound("Audio file not found on server.");

        var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            FileOptions.Asynchronous
        );

        return File(stream, opResult.Result.MimeType, enableRangeProcessing: true);
    }

    [HttpGet("next-audio/{audioId}")]
    public async Task<ActionResult<AudioFileResponse>> GetNextAudio(ObjectId audioId,
        CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        OperationResult<AudioFile> opResult =
            await _audioFileRepository.GetNextAudioFileAsync(audioId, userId.Value, cancellationToken);

        if (!opResult.IsSuccess)
        {
            return opResult.Error?.Code switch
            {
                ErrorCode.AudioNotFound => BadRequest(opResult.Error.Message),
                ErrorCode.NextAudioNotFound => BadRequest(opResult.Error.Message),
                _ => BadRequest("Operation failed!. Try again or contact administrator")
            };
        }

        bool isLiking = await _likeRepository.CheckIsLikingAsync(userId.Value, opResult.Result, cancellationToken);
        bool isAdding = await _playlistRepository.CheckIsAddingAsync(userId.Value, opResult.Result, cancellationToken);

        OperationResult<string> userNameOpResult =
            await _memberRepository.GetUserNameByObjectIdAsync(opResult.Result.UploaderId, cancellationToken);

        AudioFileResponse res = 
            Mappers.ConvertAudioFileToAudioFileResponse(opResult.Result, userNameOpResult.Result);

        return Ok(res);
    }

    [HttpGet("previous-audio/{audioId}")]
    public async Task<ActionResult<AudioFileResponse>> GetPreviousAudio(ObjectId audioId,
        CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(User.GetHashedUserId(), cancellationToken);

        if (userId is null)
            return Unauthorized("You are not logged in. Please login again");

        OperationResult<AudioFile> opResult =
            await _audioFileRepository.GetPreviousAudioFileAsync(audioId, userId.Value, cancellationToken);
        
        if (!opResult.IsSuccess)
        {
            return opResult.Error?.Code switch
            {
                ErrorCode.AudioNotFound => BadRequest(opResult.Error.Message),
                ErrorCode.PreviousAudioNotFound => BadRequest(opResult.Error.Message),
                _ => BadRequest("Operation failed!. Try again or contact administrator")
            };
        }
        
        bool isLiking = await _likeRepository.CheckIsLikingAsync(userId.Value, opResult.Result, cancellationToken);
        bool isAdding = await _playlistRepository.CheckIsAddingAsync(userId.Value, opResult.Result, cancellationToken);
        
        OperationResult<string> userNameOpResult =
            await _memberRepository.GetUserNameByObjectIdAsync(opResult.Result.UploaderId, cancellationToken);

        AudioFileResponse res = 
            Mappers.ConvertAudioFileToAudioFileResponse(opResult.Result, userNameOpResult.Result);

        return Ok(res);
    }
}