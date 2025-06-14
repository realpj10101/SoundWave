namespace api.DTOs.Track;

public record CreateAudioFile(
    string FileName,
    byte[] FileData
);