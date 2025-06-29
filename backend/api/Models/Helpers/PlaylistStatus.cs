namespace api.Models.Helpers;

public class PlaylistStatus
{
        public bool IsSuccess { get; set; }
        public  bool IsAlreadyAdded { get; set; }
        public bool IsAlreadyRemoved { get; set; }
        public bool IsTargetAudioNotFound { get; set; }
}