using api.Controllers.Helpers;
using api.DTOs;
using api.DTOs.Account;
using api.DTOs.Helpers;
using api.DTOs.Track;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

public class AudioFileController(IAudioFileRepository _audioFileRepository) : BaseApiController
{
    [HttpPost("upload/{uploaderName}")]
    public async Task<ActionResult<Response>> Upload(
        string uploaderName, [FromForm] IFormFile file, CancellationToken cancellationToken
    )
    {
        Console.WriteLine(uploaderName);

        if (file == null || file.Length == 0)
            return BadRequest("File is empty");

        if (file.Length > 40_000_000)
            return BadRequest("File is more than 40MB");

        if (!file.FileName.EndsWith(".mp3"))
            return BadRequest("Only mp3 files are allowed.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        CreateAudioFile createAudioFile = new(
            TrackUploader: uploaderName,
            FileName: file.FileName,
            FileData: ms.ToArray()
        );

        OperationResult<AudioFile> opResult = await _audioFileRepository.UploadAsync(createAudioFile, cancellationToken);

        return opResult.IsSuccess
            ? Ok(new Response(Message: "File uploaded successfully."))
            : opResult.Error?.Code switch
            {
                ErrorCode.IsNotFound => BadRequest(opResult.Error.Message),
                _ => BadRequest("Operation failed!. Try again or contact administrator")
            };
    }
}