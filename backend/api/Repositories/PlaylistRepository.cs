using api.DTOs;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Models.Helpers;
using api.Settings;
using MongoDB.Bson;
using MongoDB.Driver;

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


    public async Task<PlaylistStatus> AddAsync(ObjectId userId, string targetAudioName, CancellationToken cancellationToken)
    {
        PlaylistStatus pS = new();

        ObjectId? targetId =
            await _audioFileRepository.GetObjectIdByAudioNameAsync(targetAudioName, cancellationToken);

        if (targetId is null)
        {
            pS.IsTargetAudioNotFound = true;

            return pS;
        }
        
        bool isAdding = await _collection.Find(doc => 
                doc.AdderId == userId &&
                doc.AddedAudioId == targetId).AnyAsync(cancellationToken);

        if (isAdding)
        {
            pS.IsAlreadyAdded = true;

            return pS;
        }
        
        Playlist playlist = Mappers.ConvertPlaylistIdToPlaylist(userId, targetId.Value);

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
                audioFile.Id == targetId, updateAddersCount, null, cancellationToken);

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

    public async Task<PlaylistStatus> RemoveAsync(ObjectId userId, string targetAudioName, CancellationToken cancellationToken)
    {
        PlaylistStatus pS = new(); 
        
        ObjectId? targetId = 
            await _audioFileRepository.GetObjectIdByAudioNameAsync(targetAudioName, cancellationToken);

        if (targetId is null)
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
                && doc.AddedAudioId == targetId, cancellationToken
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
                    audioFile.Id == targetId, updateAddersCount, null, cancellationToken);

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
}