using api.DTOs;
using api.Enums;
using api.Extensions;
using api.Helpers;
using api.Interfaces;
using api.Models;
using api.Models.Helpers;
using api.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace api.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly IMongoClient _client;
    private readonly IMongoCollection<Playlist> _collection;
    private readonly ITokenService _tokenService;
    private readonly IMongoCollection<AudioFile> _collectionAudios;
    private readonly IMongoCollection<AppUser> _collectionUsers;
    private readonly IAudioFileRepository _audioFileRepository;
    private readonly ILogger<PlaylistRepository> _logger;

    public PlaylistRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings, ITokenService tokenService,
        IAudioFileRepository audioFileRepository, ILogger<PlaylistRepository> logger
    )
    {
        _client = client;
        IMongoDatabase? dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Playlist>(AppVariablesExtensions.CollectionPlaylists);
        _collectionAudios = dbName.GetCollection<AudioFile>(AppVariablesExtensions.CollectionTracks);
        _collectionUsers = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        _tokenService = tokenService;

        _audioFileRepository = audioFileRepository;

        _logger = logger;
    }


    public async Task<PlaylistStatus> AddAsync(ObjectId userId, ObjectId targetAudioId, CancellationToken cancellationToken)
    {
        PlaylistStatus pS = new();

        bool isExist = await _collectionAudios.Find(doc => doc.Id == targetAudioId).AnyAsync(cancellationToken);
        
        if (!isExist)
        {
            pS.IsTargetAudioNotFound = true;

            return pS;
        }

        bool isAdding = await _collection.Find(doc =>
                doc.AdderId == userId &&
                doc.AddedAudioId == targetAudioId).AnyAsync(cancellationToken);

        if (isAdding)
        {
            pS.IsAlreadyAdded = true;

            return pS;
        }

        Playlist playlist = Mappers.ConvertPlaylistIdToPlaylist(userId, targetAudioId);

        using IClientSessionHandle session = await _client.StartSessionAsync(null, cancellationToken);

        session.StartTransaction();

        try
        {
            await _collection.InsertOneAsync(session, playlist, null, cancellationToken);

            #region UpdateCounters

            UpdateDefinition<AppUser> updateFavoritesCount = Builders<AppUser>.Update
                .Inc(user => user.FavoritesCount, 1);

            await _collectionUsers.UpdateOneAsync(session, appUser =>
                appUser.Id == userId, updateFavoritesCount, null, cancellationToken);

            UpdateDefinition<AudioFile> updateAddersCount = Builders<AudioFile>.Update
                .Inc(audio => audio.AdderCount, 1);

            await _collectionAudios.UpdateOneAsync(session, audioFile =>
                audioFile.Id == targetAudioId, updateAddersCount, null, cancellationToken);

            #endregion

            await session.CommitTransactionAsync(cancellationToken);

            pS.IsSuccess = true;
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync(cancellationToken);

            _logger.LogError(
                "Like failed."
                + "MESSAGE:" + ex.Message
                + "TRACE:" + ex.StackTrace
            );
        }
        finally
        {
            _logger.LogInformation("MongoDB transaction/session finished.");
        }

        return pS;
    }

    public async Task<PlaylistStatus> RemoveAsync(ObjectId userId, ObjectId targetAudioId, CancellationToken cancellationToken)
    {
        PlaylistStatus pS = new();

        bool isExist = await _collectionAudios.Find(doc => doc.Id == targetAudioId).AnyAsync(cancellationToken);
        
        if (!isExist)
        {
            pS.IsTargetAudioNotFound = true;

            return pS;
        }

        using IClientSessionHandle session = await _client.StartSessionAsync(null, cancellationToken);

        session.StartTransaction();

        try
        {
            DeleteResult deleteResult = await _collection.DeleteOneAsync(
                doc => doc.AdderId == userId
                && doc.AddedAudioId == targetAudioId, cancellationToken
            );

            if (deleteResult.DeletedCount < 1)
            {
                pS.IsAlreadyRemoved = true;

                return pS;
            }

            #region UpdateCounter

            UpdateDefinition<AppUser> updateFavoritesCount = Builders<AppUser>.Update
                .Inc(appUser => appUser.FavoritesCount, -1);

            await _collectionUsers.UpdateOneAsync(session, appUser =>
                    appUser.Id == userId, updateFavoritesCount, null, cancellationToken);

            UpdateDefinition<AudioFile> updateAddersCount = Builders<AudioFile>.Update
                .Inc(audio => audio.AdderCount, -1);

            await _collectionAudios.UpdateOneAsync(session, audioFile =>
                    audioFile.Id == targetAudioId, updateAddersCount, null, cancellationToken);

            #endregion

            await session.CommitTransactionAsync(cancellationToken);

            pS.IsSuccess = true;
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync(cancellationToken);

            _logger.LogError(
                "Like failed."
                + "MESSAGE:" + ex.Message
                + "TRACE:" + ex.StackTrace
            );
        }
        finally
        {
            _logger.LogInformation("MongoDB transaction/session finished.");
        }

        return pS;
    }

    public async Task<bool> CheckIsAddingAsync(ObjectId userId, AudioFile audioFile, CancellationToken cancellationToken) =>
        await _collection.Find<Playlist>(
            doc => doc.AdderId == userId && doc.AddedAudioId == audioFile.Id
        ).AnyAsync(cancellationToken);

    public async Task<PagedList<AudioFile>> GetAllAsync(PlaylistParams playlistParams, CancellationToken cancellationToken)
    {
        if (playlistParams.Predicate == PlaylistPredicateEnum.Addings)
        {
            IQueryable<AudioFile> query = _collection.AsQueryable()
                .Where(playlist => playlist.AdderId == playlistParams.UserId)
                .Join(_collectionAudios.AsQueryable<AudioFile>(),
                    playlist => playlist.AddedAudioId,
                    audio => audio.Id,
                    (playlist, audio) => audio);

            return await PagedList<AudioFile>
                .CreatePagedListAsync(query, playlistParams.PageNumber, playlistParams.PageSize, cancellationToken);
        }

        return [];
    }

    public async Task<int> GetPlaylistsCount(string targetAudioName, CancellationToken cancellationToken)
    {
        int playlistCounts = await _collectionAudios.AsQueryable()
            .Where(doc => doc.FileName == targetAudioName)
            .Select(item => item.AdderCount)
            .FirstOrDefaultAsync(cancellationToken);

        return playlistCounts;
    }
}