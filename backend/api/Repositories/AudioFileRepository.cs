using api.DTOs;
using api.DTOs.Account;
using api.DTOs.Helpers;
using api.DTOs.Track;
using api.Extensions;
using api.Helpers;
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

    public async Task<PagedList<AudioFile>?> GetAllAsync(AudioFileParams audioFileParams, CancellationToken cancellationToken)
    {
        PagedList<AudioFile> audioFiles = await PagedList<AudioFile>.CreatePagedListAsync(
            CreateQuery(audioFileParams), audioFileParams.PageNumber, audioFileParams.PageSize, cancellationToken
        );

        return audioFiles;
    }

    public Task<OperationResult<AudioFileResponse>> GetByTrackNameAsync(string trackName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<OperationResult<AudioFile>> UploadAsync(CreateAudioFile audio, ObjectId? userId, CancellationToken cancellationToken)
    {
        string? userName = await _collectionUsers.AsQueryable()
            .Where(doc => doc.Id == userId)
            .Select(doc => doc.NormalizedUserName)
            .FirstOrDefaultAsync(cancellationToken);

        if (userName is null)
        {
            return new OperationResult<AudioFile>(
                false,
                Error: new CustomError(
                    Code: ErrorCode.IsNotFound,
                    Message: "User is not found"
                )
            );
        }

        AudioFile audioFile = Mappers.ConvertCreateAudioToAudio(audio, userName);

        await _collection.InsertOneAsync(audioFile, null, cancellationToken);

        return new OperationResult<AudioFile>(
            true,
            audioFile,
            Error: null
        );
    }

    private IMongoQueryable<AudioFile> CreateQuery(AudioFileParams audioFileParams)
    {
        IMongoQueryable<AudioFile> query = _collection.AsQueryable();

        if (!string.IsNullOrEmpty(audioFileParams.Search))
        {
            audioFileParams.Search = audioFileParams.Search.ToUpper();

            query = query.Where(
                audio => audio.FileName.Contains(audioFileParams.Search, StringComparison.CurrentCultureIgnoreCase)
                || audio.UploaderName.Contains(audioFileParams.Search, StringComparison.CurrentCultureIgnoreCase)
            );
        }

        return query;
    }
}