using api.Models.Helpers;
using MongoDB.Bson;

namespace api.Interfaces;

public interface IPlaylistRepository
{
    public Task<PlaylistStatus> AddAsync(ObjectId userId, string targetAudioName, CancellationToken cancellationToken);
    public Task<PlaylistStatus> RemoveAsync(ObjectId userId, string targetAudioName, CancellationToken cancellationToken);
}