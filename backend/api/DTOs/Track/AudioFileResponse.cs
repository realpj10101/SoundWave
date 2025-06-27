namespace api.DTOs.Track;

public record AudioFileResponse(
    string UploaderName,
    string FileName,
    string FilePath,
    bool IsLiking,
    int LikersCount,
    // byte[] FileData,
    DateTime UploadedAt
);
