namespace api.DTOs.Track;

public record AudioFileResponse(
    string UploaderName,
    string FileName,
    string FileDataBase64,
    // byte[] FileData,
    DateTime UploadedAt
);
