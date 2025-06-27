using MongoDB.Bson;

namespace api.Interfaces;

public interface IAudioService
{
    public Task<string?> SaveAudioToDiskAsync(IFormFile audiofile, ObjectId userId);
    public bool DeleteAudioFromDisk(string relativePath);    
}