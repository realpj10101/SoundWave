using System.Data.Common;
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
    private readonly IPhotoService _photoService;

    public AudioFileRepository(IMongoClient client, IMyMongoDbSettings dbSettings, IAudioService audioService, IPhotoService photoService)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AudioFile>(AppVariablesExtensions.CollectionTracks);
        _collectionUsers = database.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        _audioservice = audioService;
        _photoService = photoService;
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
            .FirstOrDefaultAsync(cancellationToken);

        return ValidationsExtensions.TestValidateObjectId(audioId);
    }

    public async Task<PagedList<AudioFile>?> GetUserAudioFiles(ObjectId? userId, AudioFileParams audioFileParams, CancellationToken cancellationToken)
    {
        IQueryable<AudioFile> query = _collection.AsQueryable();

        query = query.Where(doc => doc.Id == userId);

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

        ObjectId trackId = ObjectId.GenerateNewId();

        Task<string?> saveAudioTask = _audioservice.SaveAudioToDiskAsync(audio.File, userId.Value);

        Task<string[]?> savePhotoTask = _photoService.AddPhotoToDiskAsync(audio.CoverFile, trackId);

        await Task.WhenAll(saveAudioTask, savePhotoTask);

        string? filePath = await saveAudioTask;
        string[]? photoUrls = await savePhotoTask;

        if (filePath is null)
        {
            return new(
                false,
                Error: new(
                    ErrorCode.SaveAudioFailed,
                    "Saving audio to disk failed."
                )
            );
        }

        if (photoUrls is null)
        {
            return new(
                false,
                Error: new(
                    ErrorCode.SavePhotoFailed,
                    "Save photo to disk failed."
                )
            );
        }

        TimeSpan duration = TimeSpan.Zero;
        try
        {
            string physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
            using var tfile = TagLib.File.Create(physicalPath);
            duration = tfile.Properties.Duration;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Metadata Error: {ex.Message}");
        }

        AudioFile audioFile = new(
            Id: trackId,
            UploaderId: userId.Value,
            FileName: audio.FileName,
            FilePath: filePath,
            CoverPath: Mappers.ConvertMainPhotoUrlsToPhoto(photoUrls),
            LikersCount: 0,
            AdderCount: 0,
            CommentsCount: 0,
            UploadedAt: DateTime.UtcNow,
            Genres: audio.Genres,
            Moods: audio.Moods,
            Energy: 0.0,
            TempoBpm: 0,
            Tags: audio.Tags,
            Duration: duration.TotalSeconds
        );

        await _collection.InsertOneAsync(audioFile, null, cancellationToken);

        return new(
            true,
            audioFile,
            null
        );
    }

    private IQueryable<AudioFile> CreateQuery(AudioFileParams audioFileParams)
    {
        IQueryable<AudioFile> query = _collection.AsQueryable();

        if (!string.IsNullOrEmpty(audioFileParams.Search))
        {
            audioFileParams.Search = audioFileParams.Search.ToUpper();

            query = query.Where(
                audio => audio.FileName.Contains(audioFileParams.Search, StringComparison.CurrentCultureIgnoreCase)
            );
        }

        return query;
    }

    public async Task<List<AudioFile>> RecommendAsync(AiFilterDto f, CancellationToken cancellationToken)
    {
        var filter = Builders<AudioFile>.Filter.Empty;
        var fb = Builders<AudioFile>.Filter;
        var filters = new List<FilterDefinition<AudioFile>>();

        if (f.Moods is { Count: > 0 })
        {
            filters.Add(fb.AnyIn(x => x.Moods, f.Moods));
        }

        if (f.Genres is { Count: > 0 })
        {
            filters.Add(fb.AnyIn(x => x.Genres, f.Genres));
        }

        if (f.EnergyRange is not null)
        {
            filters.Add(
                fb.Gte(item => item.Energy, f.EnergyRange.Value.Min) &
                fb.Lte(item => item.Energy, f.EnergyRange.Value.Max)
            );
        }

        if (f.TempoRange is not null)
        {
            filters.Add(
                fb.Gte(item => item.TempoBpm, f.TempoRange.Value.Min) &
                fb.Lte(item => item.TempoBpm, f.TempoRange.Value.Max)
            );
        }

        if (!string.IsNullOrWhiteSpace(f.Text))
        {
            var text = f.Text.Trim();

            filters.Add(
                fb.Or(
                    fb.Regex(item => item.FileName, new MongoDB.Bson.BsonRegularExpression(text, "i")),
                    // fb.Regex(item => item.UploaderName, new MongoDB.Bson.BsonRegularExpression(text, "i")),
                    fb.AnyIn(item => item.Tags!, text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                )
            );
        }

        var finalFilter = filters.Count > 0
            ? fb.And(filters)
            : fb.Empty;

        var query = _collection.Find(finalFilter);

        if (string.Equals(f.Sort, "popular", StringComparison.OrdinalIgnoreCase))
        {
            query = query.SortByDescending(item => item.LikersCount);
        }
        else if (string.Equals(f.Sort, "recent", StringComparison.OrdinalIgnoreCase))
        {
            query = query.SortByDescending(item => item.UploadedAt);
        }
        else
        {
            query = query.SortByDescending(item => item.LikersCount);
        }

        int limit = f.Limit > 0 ? f.Limit : 30;

        return await query
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }
}

