namespace api.DTOs;

public record CreateCommentDto(
    string Content
);

public record CommentResponse(
    string CommentId,
    MemberDto CommentOwner,
    string Content,
    DateTime CreatedAt
);