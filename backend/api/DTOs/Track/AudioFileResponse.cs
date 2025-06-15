namespace api.DTOs.Track;

public record AudioFileResponse (
    string UploaderName,
    string FileName,
    byte[] FileData,
    DateTime UploadedAt
);
