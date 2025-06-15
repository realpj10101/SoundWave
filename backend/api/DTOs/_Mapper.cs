using api.DTOs.Account;
using api.DTOs.Track;
using api.Models;
using MongoDB.Bson;

namespace api.DTOs;

public static class Mappers
{
    public static AppUser ConvertRegisterDtoToAppUser(RegisterDto registerDto)
    {
        return new AppUser
        {
            Email = registerDto.Email,
            UserName = registerDto.UserName,
        };
    }

    public static LoggedInDto ConvertAppUserToLoggedInDto(AppUser appUser, string tokenValue)
    {
        return new LoggedInDto
        {
            Token = tokenValue,
            UserName = appUser.UserName
        };
    }

    public static AudioFile ConvertCreateAudioToAudio(CreateAudioFile createAudio, string userName)
    {
        return new AudioFile(
            UploaderName: userName,
            FileName: createAudio.FileName,
            FileData: createAudio.FileData,
            UploadedAt: DateTime.UtcNow
        );
    }

    public static AudioFileResponse ConvertAudioFileToAudioFileResponse(AudioFile audioFile, string userName)
    {
        return new AudioFileResponse(
            UploaderName: userName,
            FileName: audioFile.FileName,
            FileData: audioFile.FileData,
            UploadedAt: audioFile.UploadedAt
        );
    }
}