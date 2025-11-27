using System.Text.Json;
using api.DTOs;
using api.Interfaces;
using api.Settings;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace api.Services;

public class AiNluService : IAiNluService
{
    private readonly GrokSettings _settings;
    private readonly HttpClient _client;

    public AiNluService(IHttpClientFactory client, IOptions<GrokSettings> settings)
    {
        _client = client.CreateClient("GroqClient");
        _settings = settings.Value;
    }

    public async Task<AiFilterDto> ExtractFiltersAsync(string userPrompt, CancellationToken cancellationToken)
    {
        string system = """
    You are a music search NLU.

    You MUST respond with a SINGLE JSON object with EXACTLY two top-level fields:

    {
      "filters": {
        "moods": string[] | null,
        "genres": string[] | null,
        "energyRange": { "min": number, "max": number } | null,
        "tempoRange": { "min": number, "max": number } | null,
        "language": string | null,
        "yearRange": { "min": number, "max": number } | null,
        "text": string | null,
        "limit": number,
        "sort": "relevance" | "popular" | "recent"
      },
      "message": string
    }

    - "filters" is a compact machine-readable object for search.
    - "message" is a short, friendly explanation (1–2 sentences) of why you chose these filters.
    - Do NOT output anything outside this JSON.
    """;

        var payload = new
        {
            model = _settings.Model,
            messages = new[]
            {
                new { role = "system", content = system },
                new { role = "user", content = userPrompt }
            },
            response_format = new
            {
                type = "json_object"
            }
        };

        using var response = await _client.PostAsJsonAsync("chat/completions", payload, cancellationToken);

        Console.WriteLine(response);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        Console.WriteLine("Groq body: " + body);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Groq error {(int)response.StatusCode}: {body}");

        using var rootDoc = JsonDocument.Parse(body);

        string json = rootDoc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";

        Console.WriteLine("AI RAW JSON: " + json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var filterElements = root.GetProperty("filters");

        string? message = root.TryGetProperty("message", out var msgEl) && 
            msgEl.ValueKind == JsonValueKind.String ? msgEl.GetString() : null;

        List<string>? arr(JsonElement e, string name)
            => e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Array
            ? p.EnumerateArray().Select(item => item.GetString()!).Where(item => !string.IsNullOrWhiteSpace(item)).ToList() : null;
        
        (double Min, double Max)? dblRange(JsonElement e, string name)
        {
            if (!e.TryGetProperty(name, out var p) || p.ValueKind != JsonValueKind.Object)
                return null;

            if (!p.TryGetProperty("min", out var min) || min.ValueKind != JsonValueKind.Number)
                return null;

            if (!p.TryGetProperty("max", out var max) || max.ValueKind != JsonValueKind.Number)
                return null;

            return (min.GetDouble(), max.GetDouble());
        }

        (int Min, int Max)? intRange(JsonElement e, string name)
        {
            if (!e.TryGetProperty(name, out var p) || p.ValueKind != JsonValueKind.Object)
                return null;

            if (!p.TryGetProperty("min", out var min) || min.ValueKind != JsonValueKind.Number)
                return null;

            if (!p.TryGetProperty("max", out var max) || max.ValueKind != JsonValueKind.Number)
                return null;

            return (min.GetInt32(), max.GetInt32());
        }

        string? getStr(string name)
            => root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

        int getInt(string name, int def)
           => root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number ? p.GetInt32() : def;

        return new AiFilterDto(
            Moods: arr(root, "moods"),
            Genres: arr(root, "genres"),
            EnergyRange: dblRange(root, "energyRange"),
            TempoRange: intRange(root, "tempoRange"),
            Language: getStr("language"),
            YearRange: intRange(root, "yearRange"),
            Text: getStr("text"),
            Limit: getInt("limit", 30),
            Sort: getStr("sort"),
            Message: message
        );
    }
}