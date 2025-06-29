using System.Runtime.InteropServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace api.Models;

public record Playlist(
    [Optional][property: BsonId, BsonRepresentation(BsonType.ObjectId)] ObjectId Id,
    ObjectId AdderId,
    ObjectId AddedAudioId
);