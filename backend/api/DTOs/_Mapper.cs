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

    public static AudioFile ConvertCreateTrackToTrack(CreateAudioFile createAudio, ObjectId? uploaderId)
    {
        return new AudioFile(
            UploaderId: uploaderId,
            FileName: createAudio.FileName,
            FileData: createAudio.FileData,
            UploadedAt: DateTime.UtcNow
        );
    }
}