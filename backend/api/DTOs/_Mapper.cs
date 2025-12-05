using api.DTOs.Account;
using api.DTOs.Track;
using api.Models;
using MongoDB.Bson;

namespace api.DTOs;

public static class Mappers
{
    public static AppUser ConvertRegisterDtoToAppUser(RegisterDto registerDto) =>
        new()
        {
            Email = registerDto.Email,
            UserName = registerDto.UserName,
            Photos = []
        };

    public static LoggedInDto ConvertAppUserToLoggedInDto(AppUser appUser, string tokenValue) =>
        new()
        {
            Token = tokenValue,
            UserName = appUser.UserName,
            ProfilePhotoUrl = appUser.Photos.FirstOrDefault(photo => photo.IsMain)?.Url_165
        };

    public static MemberDto ConvertAppUserToMemberDto(AppUser appUser)
    {
        return new MemberDto(
            UserName: appUser.NormalizedUserName!,
            Bio: appUser.Bio,
            Photos: appUser.Photos
        );
    }

    public static AudioFileResponse ConvertAudioFileToAudioFileResponse(this AudioFile audioFile, bool isLiking = false, bool isAdding = false) =>
        new(
            UploaderName: audioFile.UploaderName,
            FileName: audioFile.FileName,
            FilePath: audioFile.FilePath,
            IsLiking: isLiking,
            IsAdding: isAdding,
            LikersCount: audioFile.LikersCount,
            AddersCount: audioFile.AdderCount,
            UploadedAt: audioFile.UploadedAt
        );

    public static Like ConvertLikeIdsToLike(ObjectId likerId, ObjectId likedId) =>
        new(
            LikerId: likerId,
            LikedAudioId: likedId
        );

    public static Playlist ConvertPlaylistIdToPlaylist(ObjectId adderId, ObjectId addedId) =>
     new(
            AdderId: adderId,
            AddedAudioId: addedId
        );

    public static Photo ConvertPhotoUrlsToPhoto(string[] photoUrls, bool isMain) =>
      new(
            Url_165: photoUrls[0],
            Url_256: photoUrls[1],
            Url_enlarged: photoUrls[2],
            IsMain: isMain
        );

    public static MainPhoto ConvertMainPhotoUrlsToPhoto(string[] photoUrls) =>
        new(
            Url_165: photoUrls[0],
            Url_256: photoUrls[1],
            Url_enlarged: photoUrls[2]
        );
}