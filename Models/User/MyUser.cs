using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FetchApiTutorial.Models.User
{
    public class MyUser
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId), BsonRequired] public ObjectId Id { get; set; }
        [BsonRequired,] public string Username { get; set; }
        [BsonRequired] public string Password { get; set; }

    }
}
