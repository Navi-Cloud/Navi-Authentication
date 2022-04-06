using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NaviAuth.Model.Data;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string UserEmail { get; set; }

    public string UserPassword { get; set; }
}