using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;

namespace api.Models;

[CollectionName("users")]
public class AppUser : MongoIdentityUser<ObjectId>
{
    public string? IdentifierHash { get; init; }
    public string Bio { get; init; } = string.Empty;
    public DateTime LastActive { get; init; }
    public int LikingsCount { get; init; }
    // public Photo Photo { get; set; } = new Photo(
    //     Url_165: "",
    //     Url_256: "",
    //     Url_enlarged: "",
    //     IsMain: false
    // );
    public List<Photo> Photos { get; init; } = [];
}