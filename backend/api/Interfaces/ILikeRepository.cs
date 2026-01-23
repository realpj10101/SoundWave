using api.Helpers;
using api.Models;
using api.Models.Helpers;
using MongoDB.Bson;

namespace api.Interfaces;

public interface ILikeRepository
{
    public Task<LikeStatus> CreateAsync(ObjectId userId, ObjectId targetAudioId, CancellationToken cancellationToken);
    public Task<LikeStatus> DeleteAsync(ObjectId userId, ObjectId targetAudioId, CancellationToken cancellationToken);
    public Task<bool> CheckIsLikingAsync(ObjectId userId, AudioFile audioFile, CancellationToken cancellationToken);
    public Task<PagedList<AudioFile>> GetAllAsync(LikeParams likeParams, CancellationToken cancellationToken);
    public Task<int> GetLikesCount(string targetAudioName, CancellationToken cancellationToken);
}