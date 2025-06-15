using System.Security.Cryptography.X509Certificates;
using api.DTOs.Helpers;
using api.DTOs.Track;
using api.Models;
using MongoDB.Bson;

namespace api.Interfaces;

public interface IAudioFileRepository
{
    public Task<OperationResult<AudioFile>> GetByTrackNameAsync(string audioName, CancellationToken cancellationToken);
    public Task<IEnumerable<AudioFile>> GetAllAsync(CancellationToken cancellationToken);
    public Task<OperationResult<AudioFile>> UploadAsync(CreateAudioFile audio, ObjectId? userId, CancellationToken cancellationToken);
}