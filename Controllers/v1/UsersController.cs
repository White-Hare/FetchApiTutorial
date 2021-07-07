using System;
using System.Net;
using FetchApiTutorial.Models.User;
using FetchApiTutorial.Services.UserService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FetchApiTutorial.Helpers;
using FetchApiTutorial.Helpers.Attributes;
using FetchApiTutorial.Helpers.Settings;
using FetchApiTutorial.Models.User.RequestsAndResponses;
using Microsoft.AspNetCore.Http;


namespace FetchApiTutorial.Controllers.v1
{
    [ApiController, Route("api/v1/[controller]"), Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Authenticate"), AllowAnonymous]
        public async Task<ActionResult<AuthenticateResponse>> Authenticate(AuthenticationRequest model)
        {
            var response = await _userService.Authenticate(model, IpAddress());
            if (response == null)
                return BadRequest("Login Failed");

            SetTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        [HttpPost("Refresh"), AllowAnonymous]
        public async Task<ActionResult<AuthenticateResponse>> RefreshToken()
        {
            var token = Request.Cookies[JwtSettings.RefreshTokenKey];
            if (token == null)
                return NotFound();

            var response = await _userService.RefreshToken(token, IpAddress());

            if (response != null)
            {
                SetTokenCookie(response.RefreshToken);
                return Ok(response);
            }
            else
                return BadRequest("No Token Found To Refresh");
        }

        [HttpPost("Revoke")]
        public async Task<ActionResult<RevokeTokenRequest>> RevokeToken(RevokeTokenRequest model)
        {
            // accept refresh token in request body or cookie
            var token = model.Token ?? Request.Cookies[JwtSettings.RefreshTokenKey];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            RefreshToken request = await _userService.RevokeToken(token, IpAddress());
            if (request == null)
                return BadRequest("Revoke Failed");


            return Ok("Revoke Successful"); //new { message = "Token revoked" });
        }

        [HttpPost("Register"), AllowAnonymous]//Fix it
        public async Task<ActionResult<MyUser>> Register(AuthenticationRequest request)
        {
            var response = await _userService.RegisterUserAsync(request);

            if (response == null)
                return BadRequest(new { message = "Failed to create user" });

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }

        [HttpGet("{id}/refresh-tokens")]
        public async Task<IActionResult> GetRefreshTokens(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(user.RefreshTokens);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
        private void SetTokenCookie(string token)
        {
            // append cookie with refresh token to the http response
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(2)
            };


            Response.Cookies.Append(JwtSettings.RefreshTokenKey, token, cookieOptions);
        }

        private string IpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();        }
    }
}
