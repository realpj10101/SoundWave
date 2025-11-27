namespace api.DTOs;

public record AiFilterDto(
    List<string>? Moods,
    List<string>? Genres,
    (double Min, double Max)? EnergyRange,
    (int Min, int Max)? TempoRange,
    string? Language,
    (int Min, int Max)? YearRange,
    string? Text,
    int Limit = 30,
    string? Sort = "relevance",
    string? Message = ""
);