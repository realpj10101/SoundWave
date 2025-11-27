using api.DTOs.Track;

namespace api.DTOs;

public record AiRecommendResponse(
    string? Message,
    List<AudioFileResponse> Items
);