using api.Extensions.Validations;
using api.Validations;

namespace api.DTOs.Track;

public record CreateAudioFile(
    string FileName,
    [AllowedAudioFilesExtensions(
        ErrorMessage = "Invalid audio format. Allowed formats: .mp3, .wav, .ogg, .opus, .m4a, .aac, .webm."
    )]
    [FileSize(250_000, 30_000_000, 
        ErrorMessage = "Audio file size must be between 250 KB and 30 MB."
    )]
    IFormFile File,
    [AllowedFileExtensions(
        ErrorMessage = "Invalid image format. Allowed formats: .jpg, .jpeg .png, .webp, .avif."
    )]
    [FileSize(10_000, 5_000_000, 
        ErrorMessage = "Cover image size must be between 10 KB and 5 MB."
    )]
    IFormFile CoverFile,
    List<string> Genres,
    List<string> Moods,
    List<string> Tags
);