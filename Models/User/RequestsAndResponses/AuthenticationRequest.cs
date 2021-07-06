
using MongoDB.Bson.Serialization.Attributes;

namespace FetchApiTutorial.Models.User.RequestsAndResponses
{
    public class AuthenticationRequest
    {
        [BsonRequired] public string Username { get; set; }
        [BsonRequired] public string Password { get; set; }
    }
}
