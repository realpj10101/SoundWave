using api.DTOs;
using api.DTOs.Account;
using api.DTOs.Helpers;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Models.Helpers;
using api.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using TagLib.Riff;

namespace api.Repositories;

public class CommentRepository : ICommentRepository
{
    #region DB and vars
    
    private readonly IMongoClient _client;
    private readonly IMongoCollection<Comment> _collection;
    private readonly IMongoCollection<AudioFile> _collectionAudio;
    private readonly IMongoCollection<AppUser>  _collectionUser;

    public CommentRepository(
        IMongoClient client, IMyMongoDbSettings dbSettings, ITokenService tokenService
    )
    {
        _client = client;
        IMongoDatabase? dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Comment>(AppVariablesExtensions.CollectionComments);
        _collectionAudio = dbName.GetCollection<AudioFile>(AppVariablesExtensions.CollectionTracks);
        _collectionUser = dbName.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);
    }

    #endregion
        
    public async Task<OperationResult<CommentResponse>> CreateAsync(ObjectId userId, ObjectId targetAudioId, string content, CancellationToken cancellationToken)
    {
        bool isExist = await _collectionAudio.Find(doc => doc.Id == targetAudioId).AnyAsync(cancellationToken);

        if (!isExist)
        {
            return new(
                false,
                Error: new(
                    ErrorCode.IsNotFound,
                    "Target audio does not exist."
                    )
                );
        }
        
        Comment comment = Mappers.ConvertCommentIdToComment(userId, targetAudioId, content);
        
        AppUser? appUser = await _collectionUser.Find(doc => doc.Id == userId).FirstOrDefaultAsync(cancellationToken);

        if (appUser == null)
        {
            return new(
                false,
                Error: new CustomError(
                    ErrorCode.IsUserNotFound,
                    "User not found!")
                );
        }

        MemberDto member = Mappers.ConvertAppUserToMemberDto(appUser);

        using IClientSessionHandle session = await _client.StartSessionAsync(null, cancellationToken);

        session.StartTransaction();

        try
        {
            await _collection.InsertOneAsync(session, comment, null, cancellationToken);
        
            #region UpdateCounters

            UpdateDefinition<AudioFile> updateCommentsCount = Builders<AudioFile>.Update
                .Inc(doc => doc.CommentsCount, 1);
            
            await _collectionAudio.UpdateOneAsync(session, doc => doc.Id == targetAudioId, updateCommentsCount, null, cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);

            #endregion
        }
        catch (Exception ex)
        {
            Console.WriteLine("exception");
            
            await session.AbortTransactionAsync(cancellationToken);
        }

        return new(
            true,
            Mappers.ConvertCommentToCommentResponse(comment, member),
            null
        );
    }

    public async Task<OperationResult<IEnumerable<CommentResponse>>> GetAllAudioCommentsAsync(ObjectId audioId, CancellationToken cancellationToken)
    {
        IEnumerable<Comment> comments = await _collection.Find(doc => doc.AudioId == audioId)
            .SortByDescending(doc => doc.CreatedAt)
            .ToListAsync(cancellationToken);
        
        if (!comments.Any())
        {
            return new(
                false,
                [],
                null
            );
        }
        
        List<CommentResponse> commentResponses = [];

        foreach (Comment comment in comments)
        {
            AppUser? appUser = await _collectionUser
                .Find(doc => doc.Id == comment.UserId).FirstOrDefaultAsync(cancellationToken);

            MemberDto memberDto = Mappers.ConvertAppUserToMemberDto(appUser);
            
            CommentResponse res = Mappers.ConvertCommentToCommentResponse(comment, memberDto);
            
            commentResponses.Add(res);
        }

        return new(
            true,
            commentResponses,
            null);
    }
}