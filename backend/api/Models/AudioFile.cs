using System.Runtime.InteropServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models;

public record AudioFile(
    [Optional]
    [property: BsonId, BsonRepresentation(BsonType.ObjectId)]
    ObjectId Id,
    ObjectId UploaderId,
    string FileName,
    string FilePath,
    MainPhoto CoverPath,
    int LikersCount,
    int AdderCount,
    int CommentsCount,
    DateTime UploadedAt,
    List<string> Genres,
    List<string> Moods,
    double Energy,
    int TempoBpm,
    List<string> Tags,
    double Duration
);