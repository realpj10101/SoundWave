namespace api.DTOs.Track;

public record AudioFileResponse(
    string UploaderName,
    string FileName,
    string FilePath,
    bool IsLiking,
    bool IsAdding,
    int LikersCount,
    int AddersCount,
    DateTime UploadedAt
);
