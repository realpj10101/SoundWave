namespace api.DTOs.Track;

public record AudioFileResponse(
    string UploaderName,
    string FileName,
    string FileDataBase64,
    bool IsLiking,
    int LikersCount,
    // byte[] FileData,
    DateTime UploadedAt
);
