using FetchApiTutorial.Models.User;
using FetchApiTutorial.Services.UserService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FetchApiTutorial.Helpers;
using Microsoft.AspNetCore.Http;


namespace FetchApiTutorial.Controllers.v1
{
    [ApiController, Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Authenticate")]
        public async Task<ActionResult<AuthenticateResponse>> Authenticate(AuthenticationRequest model)
        {
            var response = await _userService.Authenticate(model);
            
            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            Response.Cookies.Append(JwtMiddleware.TokenName, response.Token);
            return Ok(response);
        }


        [HttpPost("Register")]//Fix it
        public async Task<ActionResult<MyUser>> Register(AuthenticationRequest request)
        {
            var response = await _userService.RegisterUserAsync(request);

            if (response == null)
                return BadRequest(new { message = "Failed to create user" });

            return Ok(response);
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

    }
}
