using api.Interfaces;
using MongoDB.Bson;

namespace api.Services;

public class AudioService : IAudioService
{
    private const string wwwRootPath = "wwwroot/";
    private const string audioRoot = "storage/audios/";

    public async Task<string?> SaveAudioToDiskAsync(IFormFile audiofile, ObjectId userId)
    {
        if (audiofile is null || audiofile.Length == 0)
            return null;

        string userFolder = Path.Combine(audioRoot, userId.ToString(), "original");
        string absolutePath = Path.Combine(wwwRootPath, userFolder);
        Directory.CreateDirectory(absolutePath);

        string fileName = Path.GetFileName(audiofile.FileName);
        string filePath = Path.Combine(absolutePath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await audiofile.CopyToAsync(stream);

        return Path.Combine(userFolder, fileName).Replace("\\", "/");
    }

       public bool DeleteAudioFromDisk(string relativePath)
    {
        throw new NotImplementedException();
    }
}