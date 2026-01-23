using api.DTOs;
using api.DTOs.Helpers;
using api.DTOs.Track;
using api.Interfaces;
using MongoDB.Bson;

namespace api.Services;

public class AiRecommendService : IAiRecommendService
{
    private readonly IAiNluService _nluService;
    private readonly IAudioFileRepository _audioFileRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IMemberRepository _memberRepository;

    public AiRecommendService(
        IAiNluService aiNluService, IAudioFileRepository audioFileRepository, ILikeRepository likeRepository,
        IPlaylistRepository playlistRepository, IMemberRepository memberRepository
    )
    {
        _nluService = aiNluService;
        _audioFileRepository = audioFileRepository;
        _likeRepository = likeRepository;
        _playlistRepository = playlistRepository;
        _memberRepository = memberRepository;
    }

    public async Task<AiRecommendResponse> RecommendAsync(string userPrompt, ObjectId userId, CancellationToken cancellationToken)
    {
        var filters = await _nluService.ExtractFiltersAsync(userPrompt, cancellationToken);

        var items = await _audioFileRepository.RecommendAsync(filters, cancellationToken);

        List<AudioFileResponse> audioFileResponses = new List<AudioFileResponse>(items.Count);
        foreach (var audio in items)
        {
            bool isLiking = await _likeRepository.CheckIsLikingAsync(userId, audio,cancellationToken);
            bool isAdding = await _playlistRepository.CheckIsAddingAsync(userId, audio, cancellationToken);

            OperationResult<string> opResult =
                await _memberRepository.GetUserNameByObjectIdAsync(audio.UploaderId, cancellationToken);

            audioFileResponses.Add(Mappers.ConvertAudioFileToAudioFileResponse(audio, opResult.Result, isLiking, isAdding));
        }

        return new(
            Message: filters.Message,
            Items: audioFileResponses
        );
    }
}