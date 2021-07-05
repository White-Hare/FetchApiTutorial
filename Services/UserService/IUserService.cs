using FetchApiTutorial.Models.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FetchApiTutorial.Services.UserService
{
    public interface IUserService
    {
        Task<AuthenticateResponse> Authenticate(AuthenticationRequest model);
        Task<IEnumerable<MyUser>> GetAllAsync();
        Task<MyUser> RegisterUserAsync(AuthenticationRequest request);
        Task<MyUser> GetByIdAsync(string id);
    }
}
