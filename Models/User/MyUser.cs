using MongoDB.Bson.Serialization.Attributes;

namespace FetchApiTutorial.Models.User
{
    public class MyUser
    {
        [BsonId, BsonRequired] public string Id { get; set; }
        [BsonRequired,] public string Username { get; set; }
        [BsonRequired] public string Password { get; set; }

    }
}
