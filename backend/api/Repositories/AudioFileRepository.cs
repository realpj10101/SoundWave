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
    private readonly IAudioService _audioservice;

    public AudioFileRepository(IMongoClient client, IMyMongoDbSettings dbSettings, IAudioService audioService)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AudioFile>(AppVariablesExtensions.CollectionTracks);
        _collectionUsers = database.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        _audioservice = audioService;
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

    public async Task<ObjectId?> GetObjectIdByAudioNameAsync(string audioName, CancellationToken cancellationToken)
    {
        ObjectId? audioId = await _collection.AsQueryable()
            .Where(audio => audio.FileName == audioName)
            .Select(item => item.Id)
            .SingleOrDefaultAsync(cancellationToken);

        return ValidationsExtensions.TestValidateObjectId(audioId);
    }

    public async Task<PagedList<AudioFile>?> GetUserAudioFiles(ObjectId? userId, AudioFileParams audioFileParams, CancellationToken cancellationToken)
    {
        string? userName = await _collectionUsers.AsQueryable()
            .Where(doc => doc.Id == userId)
            .Select(item => item.NormalizedUserName)
            .FirstOrDefaultAsync(cancellationToken);

        IMongoQueryable<AudioFile> query = _collection.AsQueryable();

        query = query.Where(doc => doc.UploaderName == userName);

        return await PagedList<AudioFile>
            .CreatePagedListAsync(query, audioFileParams.PageNumber, audioFileParams.PageSize, cancellationToken);
    }

    public async Task<OperationResult<AudioFile>> UploadAsync(CreateAudioFile audio, ObjectId? userId, CancellationToken cancellationToken)
    {
        if (userId is null)
        {
            return new OperationResult<AudioFile>(
                false,
                Error: new CustomError(
                    ErrorCode.IsFailed,
                    "user id can not be null"
                )
            );
        }

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

        string? filePath = await _audioservice.SaveAudioToDiskAsync(audio.File, userId.Value);
        if (filePath is null)
        {
            return new OperationResult<AudioFile>(
                false,
                Error: new CustomError(
                    Code: ErrorCode.SaveFailed,
                    Message: "Saving audio to disk failed"
                )
            );
        }

        AudioFile audioFile = new(
            Id: ObjectId.GenerateNewId(),
            UploaderName: userName,
            FileName: audio.FileName,
            FilePath: filePath,
            LikersCount: 0,
            UploadedAt: DateTime.UtcNow
        );

        await _collection.InsertOneAsync(audioFile, null, cancellationToken);

        return new OperationResult<AudioFile>(
            true,
            audioFile,
            null
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