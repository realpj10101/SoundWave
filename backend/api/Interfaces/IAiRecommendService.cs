using api.DTOs;
using api.DTOs.Track;
using api.Models;
using MongoDB.Bson;

namespace api.Interfaces;

public interface IAiRecommendService
{
    public Task<AiRecommendResponse> RecommendAsync(string userPrompt, ObjectId userId, CancellationToken cancellationToken);
}