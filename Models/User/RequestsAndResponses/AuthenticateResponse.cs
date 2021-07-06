using MongoDB.Bson;
using Newtonsoft.Json;

namespace FetchApiTutorial.Models.User.RequestsAndResponses
{
    public class AuthenticateResponse
    { 
        public ObjectId Id { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }
        [JsonIgnore]public string RefreshToken { get; set; }

        public AuthenticateResponse(MyUser user, string jwtJwtToken, string refreshToken)
        {
            Id = user.Id;
            Username = user.Username;
            JwtToken = jwtJwtToken;
            RefreshToken = refreshToken;
        }
    }
}
