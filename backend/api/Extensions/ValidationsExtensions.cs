using MongoDB.Bson;

namespace api.Extensions;

public static class ValidationsExtensions
{
    public static ObjectId? TestValidateObjectId(ObjectId? objectId)
    {
        return objectId is null || !objectId.HasValue || objectId.Equals(ObjectId.Empty)
            ? null
            : objectId;
    }
    
    public static bool ValidateObjectId(ObjectId? objectId) =>
        objectId.HasValue && !objectId.Equals(ObjectId.Empty);
}


// public static ObjectId? ValidateObjectId(ObjectId? objectId)
// {
//     return objectId is null || !objectId.HasValue || objectId.Equals(ObjectId.Empty)
//         ? null
//         : objectId;
// }

// public static OperationResult<bool> ValidateExObjectId(ObjectId? objectId) =>
//     new(
//         objectId.HasValue && !objectId.Equals(ObjectId.Empty)
//     );