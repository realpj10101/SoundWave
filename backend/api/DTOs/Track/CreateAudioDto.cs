namespace api.DTOs.Track;

public record CreateAudioFile(
    string FileName,
    IFormFile File,
    IFormFile CoverFile,
    List<string> Genres,
    List<string> Moods,
    List<string> Tags
);