using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace FetchApiTutorial.Models.User
{
    public class MyUser
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator)), BsonRepresentation(BsonType.ObjectId), BsonRequired] public ObjectId Id { get; set; }
        [BsonRequired,] public string Username { get; set; }
        [BsonRequired] public string Password { get; set; }
        [JsonIgnore] public List<RefreshToken> RefreshTokens { get; set; }
    }
}
