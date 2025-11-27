using System.Runtime.InteropServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models;

public record AudioFile(
    [Optional]
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)]
    ObjectId Id,
    string UploaderName,
    string FileName,
    string FilePath,
    int LikersCount,
    int AdderCount,
    DateTime UploadedAt,
    List<string>? Genres,
    List<string>? Moods,
    double? Energy,
    int? TempoBpm,
    List<string> Tags
);