
using api.Enums;
using MongoDB.Bson;

namespace api.Helpers;

public class PlaylistParams : PaginationParams
{
    public ObjectId? AudioId { get; set; }
    public ObjectId? UserId { get; set; }
    public PlaylistPredicateEnum Predicate { get; set; }
}