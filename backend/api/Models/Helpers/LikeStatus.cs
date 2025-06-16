namespace api.Models.Helpers;

public class LikeStatus
{
    public bool IsSuccess { get; set; }
    public bool IsAlreadyLiked { get; set; }
    public bool IsAlreadyDisLiked { get; set; }
    public bool IsTargetAudioNotFound { get; set; }
}