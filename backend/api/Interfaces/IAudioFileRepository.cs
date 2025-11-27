using System.Security.Cryptography.X509Certificates;
using api.DTOs;
using api.DTOs.Helpers;
using api.DTOs.Track;
using api.Helpers;
using api.Models;
using MongoDB.Bson;

namespace api.Interfaces;

public interface IAudioFileRepository
{
    public Task<OperationResult<AudioFileResponse>> GetByTrackNameAsync(string audioName, CancellationToken cancellationToken);
    public Task<PagedList<AudioFile>?> GetAllAsync(AudioFileParams audioFileParams, CancellationToken cancellationToken);
    public Task<OperationResult<AudioFile>> UploadAsync(CreateAudioFile audio, ObjectId? userId, CancellationToken cancellationToken);
    public Task<ObjectId?> GetObjectIdByAudioNameAsync(string audioName, CancellationToken cancellationToken);
    public Task<PagedList<AudioFile>?> GetUserAudioFiles(ObjectId? userId, AudioFileParams audioFileParams, CancellationToken cancellationToken);
    public Task<List<AudioFile>>RecommendAsync(AiFilterDto filters, CancellationToken cancellationToken);
}