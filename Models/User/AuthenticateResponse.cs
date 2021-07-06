namespace FetchApiTutorial.Models.User
{
    public class AuthenticateResponse
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }


        public AuthenticateResponse(MyUser user, string token)
        {
            Id = user.Id.ToString();
            Username = user.Username;
            Token = token;
        }
    }
}
