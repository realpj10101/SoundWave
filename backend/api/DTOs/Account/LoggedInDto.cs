using System.Runtime.InteropServices;

namespace api.DTOs;

public class LoggedInDto
{
    public string? Token { get; init; }
    public string? UserName { get; init; }
    // public string? Gender { get; init; }
    // public string? ProfilePhotoUrl { get; init; }
    public bool IsWrongCreds { get; set; }
    public List<string> Errors { get; init; } = [];
}