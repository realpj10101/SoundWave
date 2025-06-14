namespace api.DTOs.Track;

public record CreateAudioFile(
    string TrackUploader,
    string FileName,
    byte[] FileData
);