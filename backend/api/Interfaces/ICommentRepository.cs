using api.DTOs;
using api.DTOs.Helpers;
using api.Models;
using api.Models.Helpers;
using MongoDB.Bson;

namespace api.Interfaces;

public interface ICommentRepository
{
    public Task<OperationResult<CommentResponse>> CreateAsync(ObjectId userId, ObjectId targetAudioId, string content, CancellationToken cancellationToken);

    public Task<OperationResult<IEnumerable<CommentResponse>>> GetAllAudioCommentsAsync(ObjectId audioId,
        CancellationToken cancellationToken);
}