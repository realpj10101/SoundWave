using api.DTOs;
using api.DTOs.Account;
using api.DTOs.Helpers;
using api.DTOs.Track;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace api.Repositories;

public class AudioFileRepository : IAudioFileRepository
{
    private readonly IMongoCollection<AudioFile> _collection;
    private readonly IMongoCollection<AppUser>? _collectionUsers;    

    public AudioFileRepository(IMongoClient client, IMyMongoDbSettings dbSettings)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AudioFile>(AppVariablesExtensions.CollectionTracks);
        _collectionUsers = database.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);
    }

    public Task<IEnumerable<AudioFile>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<OperationResult<AudioFile>> GetByTrackNameAsync(string trackName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<AudioFile>> UploadAsync(CreateAudioFile audio, ObjectId? userId, CancellationToken cancellationToken)
    {
        AudioFile audioFile = Mappers.ConvertCreateTrackToTrack(audio, userId);

        await _collection.InsertOneAsync(audioFile, null, cancellationToken);

        return new OperationResult<AudioFile>(
            true,
            audioFile,
            null
        );
    }
}