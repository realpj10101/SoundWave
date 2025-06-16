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
            LikersCount: 0,
            UploadedAt: DateTime.UtcNow
        );
    }

    public static AudioFileResponse ConvertAudioFileToAudioFileResponse(this AudioFile audioFile, bool IsLiking = false)
    {
        string base64 = Convert.ToBase64String(audioFile.FileData);
        string dataUri = $"data:audio/mpeg;base64,{base64}";

        return new AudioFileResponse(
            UploaderName: audioFile.UploaderName,
            FileName: audioFile.FileName,
            FileDataBase64: dataUri,
            IsLiking: IsLiking,
            LikersCount: audioFile.LikersCount,
            UploadedAt: audioFile.UploadedAt
        );
    }

    public static Like ConvertLikeIdsToLike(ObjectId likerId, ObjectId likedId)
    {
        return new Like(
            LikerId: likerId,
            LikedAudioId: likedId
        );
    }
}