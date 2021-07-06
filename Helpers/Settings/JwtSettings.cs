namespace FetchApiTutorial.Helpers.Settings
{
    public class JwtSettings
    {
        public const string TokenKey = "Authorization";
        public const string UserKey = "User";
        public const string RefreshTokenKey = "RefreshToken";
        public string Secret { get; set; }
        public int RefreshTokenTTL { get; set; }
    }
}
