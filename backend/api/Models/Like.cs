using System.Runtime.InteropServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models;

public record Like(
    [Optional][property: BsonId, BsonRepresentation(BsonType.ObjectId)] ObjectId Id,
    ObjectId LikerId,
    ObjectId LikedAudioId
);