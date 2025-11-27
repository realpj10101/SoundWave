using api.DTOs;

namespace api.Interfaces;

public interface IAiNluService
{
    public Task<AiFilterDto> ExtractFiltersAsync(string userPrompt, CancellationToken cancellationToken); 
}