using api.Helpers;
using api.Models;
using api.Models.Helpers;
using MongoDB.Bson;

namespace api.Interfaces;

public interface IPlaylistRepository
{
    public Task<PlaylistStatus> AddAsync(ObjectId userId, ObjectId targetAudioId, CancellationToken cancellationToken);
    public Task<PlaylistStatus> RemoveAsync(ObjectId userId, ObjectId targetAudioId, CancellationToken cancellationToken);
    public Task<bool> CheckIsAddingAsync(ObjectId userId, AudioFile audioFile, CancellationToken cancellationToken);
    public Task<PagedList<AudioFile>> GetAllAsync(PlaylistParams playlistParams, CancellationToken cancellationToken);
    public Task<int> GetPlaylistsCount(string targetAudioName, CancellationToken cancellationToken);
}