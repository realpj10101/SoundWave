namespace api.Models;

public class AppUser
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
}