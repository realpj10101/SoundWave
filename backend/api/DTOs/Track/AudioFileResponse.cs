using api.Models;

namespace api.DTOs.Track;

public record AudioFileResponse(
    string UploaderName,
    string FileName,
    string FilePath,
    MainPhoto CoverPath,
    bool IsLiking,
    bool IsAdding,
    int LikersCount,
    int AddersCount,
    List<string> Genres,
    List<string> Moods,
    List<string> Tags,
    Double Duration,
    DateTime UploadedAt
);
