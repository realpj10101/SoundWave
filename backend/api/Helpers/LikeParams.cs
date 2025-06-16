using api.Enums;
using MongoDB.Bson;

namespace api.Helpers;

public class LikeParams : PaginationParams
{
    public ObjectId? AudioId { get; set; }
    
    public LikePredicateEnum Predicate { get; set; }
}