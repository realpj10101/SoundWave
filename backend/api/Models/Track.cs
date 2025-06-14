using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models;

public record Track(
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)]
    ObjectId Id,
    string UploaderName,
    string FileName,
    byte[] FileData,
    DateTime UploadedAt
);