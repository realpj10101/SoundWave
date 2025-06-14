using api.Models;

namespace api.Extensions;

public class AppVariablesExtensions
{
    public const string TokenKey = "TokenKey";

    public const string CollectionUsers = "users";
    public const string CollectionFollows = "follows";
    public const string CollectionTracks = "tracks";
    public const string CollectionComments = "comments";
    public const string CollectionLikes = "likes";
    public const string CollectionTeams = "teams";
    public const string CollectionChats = "chats";
    public const string CollectionExceptionLogs = "exception-logs";
    public const string CollectionRefreshTokens = "refresh-tokens";

    public readonly static string[] AppVersion = ["1", "1.0.2"];

    public readonly static AppRole[] roles = [
        new() {Name = Roles.admin.ToString()},
        new() {Name = Roles.moderator.ToString()},
        new() {Name = Roles.member.ToString()}
    ];
}

public enum Roles
{
    admin, 
    moderator,
    member
}

public enum FollowPredicate
{
    Followings,
    Followers
}

public enum FollowAddOrRemove
{
    IsAdded,
    IsRemoved
}