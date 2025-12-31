using api.Models;

namespace api.DTOs;

public record MemberDto(
    string UserName,
    string Bio,
    MainPhoto Photo
);