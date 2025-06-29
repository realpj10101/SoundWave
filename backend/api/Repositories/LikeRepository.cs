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

public class LikeRepository : ILikeRepository
{   
    #region DB and vars

    private readonly IMongoClient _client;
    private readonly IMongoCollection<Like> _collection;
    private readonly ITokenService _tokenService;
    private readonly IMongoCollection<AudioFile> _collectionAudios;
    private readonly IMongoCollection<AppUser> _colletionUsers;
    private readonly IAudioFileRepository _audioFileRepository;
    private readonly ILogger<LikeRepository> _logger;

    public LikeRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings, ITokenService tokenService,
        IAudioFileRepository audioFileRepository, ILogger<LikeRepository> logger
    )
    {
        _client = client;
        IMongoDatabase? dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Like>(AppVariablesExtensions.CollectionLikes);
        _collectionAudios = dbName.GetCollection<AudioFile>(AppVariablesExtensions.CollectionTracks);
        _colletionUsers = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);

        _tokenService = tokenService;

        _audioFileRepository = audioFileRepository;

        _logger = logger;
    }

    #endregion

    public async Task<LikeStatus> CreateAsync(ObjectId userId, string targetAudioName, CancellationToken cancellationToken)
    {
        LikeStatus lS = new();

        ObjectId? targetId =
            await _audioFileRepository.GetObjectIdByAudioNameAsync(targetAudioName, cancellationToken);

        if (targetId is null)
        {
            lS.IsTargetAudioNotFound = true;

            return lS;
        }

        bool isLiking = await _collection.Find<Like>(likeDoc =>
            likeDoc.LikerId == userId &&
            likeDoc.LikedAudioId == targetId)
            .AnyAsync(cancellationToken);

        if (isLiking)
        {
            lS.IsAlreadyLiked = true;

            return lS;
        }

        Like like = Mappers.ConvertLikeIdsToLike(userId, targetId.Value);

        using IClientSessionHandle session = await _client.StartSessionAsync(null, cancellationToken);

        session.StartTransaction();

        try
        {
            await _collection.InsertOneAsync(session, like, null, cancellationToken);

            #region UpdateCounters

            UpdateDefinition<AppUser> updateLikingsCount = Builders<AppUser>.Update
                .Inc(user => user.LikingsCount, 1);

            await _colletionUsers.UpdateOneAsync<AppUser>(session, appUser =>
                appUser.Id == userId, updateLikingsCount, null, cancellationToken);

            UpdateDefinition<AudioFile> updateLikersCount = Builders<AudioFile>.Update
                .Inc(audio => audio.LikersCount, 1);

            await _collectionAudios.UpdateOneAsync<AudioFile>(session, audio =>
                audio.Id == targetId, updateLikersCount, null, cancellationToken);

            #endregion

            await session.CommitTransactionAsync(cancellationToken);

            lS.IsSuccess = true;
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

        return lS;
    }

    public async Task<LikeStatus> DeleteAsync(ObjectId userId, string targetAudioName, CancellationToken cancellationToken)
    {
        LikeStatus lS = new();

        ObjectId? targetId =
            await _audioFileRepository.GetObjectIdByAudioNameAsync(targetAudioName, cancellationToken);

        if (targetId is null)
        {
            lS.IsTargetAudioNotFound = true;

            return lS;
        }

        using IClientSessionHandle session = await _client.StartSessionAsync(null, cancellationToken);

        session.StartTransaction();

        try
        {
            DeleteResult deleteResult = await _collection.DeleteOneAsync(
                doc => doc.LikerId == userId
                    && doc.LikedAudioId == targetId, cancellationToken
            );

            if (deleteResult.DeletedCount < 1)
            {
                lS.IsAlreadyDisLiked = true;

                return lS;
            }

            #region UpdateCounters

            UpdateDefinition<AppUser> updateLikingsCount = Builders<AppUser>.Update
                .Inc(appUser => appUser.LikingsCount, -1);

            await _colletionUsers.UpdateOneAsync<AppUser>(session, appUser =>
                appUser.Id == userId, updateLikingsCount, null, cancellationToken);

            UpdateDefinition<AudioFile> updateLikersCount = Builders<AudioFile>.Update
                .Inc(audio => audio.LikersCount, -1);

            await _collectionAudios.UpdateOneAsync<AudioFile>(session, audio =>
                audio.Id == targetId, updateLikersCount, null, cancellationToken);

            #endregion

            await session.CommitTransactionAsync(cancellationToken);

            lS.IsSuccess = true;
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

        return lS;
    }

    public async Task<bool> CheckIsLikingAsync(ObjectId userId, AudioFile audioFile, CancellationToken cancellationToken)
        => await _collection.Find<Like>(
            doc => doc.LikerId == userId && doc.LikedAudioId == audioFile.Id
        ).AnyAsync(cancellationToken);


    public async Task<PagedList<AudioFile>> GetAllAsync(LikeParams likeParams, CancellationToken cancellationToken)
    {
        if (likeParams.Predicate == LikePredicateEnum.Likings)
        {
            IMongoQueryable<AudioFile> query = _collection.AsQueryable<Like>()
                .Where(like => like.LikerId == likeParams.UserId)
                .Join(_collectionAudios.AsQueryable<AudioFile>(),
                    like => like.LikedAudioId,
                    audio => audio.Id,
                    (like, audio) => audio);

            return await PagedList<AudioFile>
                .CreatePagedListAsync(query, likeParams.PageNumber, likeParams.PageSize, cancellationToken);
        }

        return [];
    }

    public async Task<int> GetLikesCount(string targetAudioName, CancellationToken cancellationToken)
    {
        int likersCount = await _collectionAudios.AsQueryable()
            .Where(doc => doc.FileName == targetAudioName)
            .Select(item => item.LikersCount)
            .FirstOrDefaultAsync(cancellationToken);

        return likersCount;
    }
}