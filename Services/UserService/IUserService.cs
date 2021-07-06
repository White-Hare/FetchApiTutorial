using FetchApiTutorial.Models.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using FetchApiTutorial.Models.User.RequestsAndResponses;

namespace FetchApiTutorial.Services.UserService
{
    public interface IUserService
    {
        Task<AuthenticateResponse> Authenticate(AuthenticationRequest model, string ipAddress);
        Task<AuthenticateResponse> RefreshToken(string token, string ipAddress);
        Task RevokeToken(string token, string ipAddress);
        Task<IEnumerable<MyUser>> GetAllAsync();
        Task<MyUser> RegisterUserAsync(AuthenticationRequest request);
        Task<MyUser> GetByIdAsync(string id);
    }
}
