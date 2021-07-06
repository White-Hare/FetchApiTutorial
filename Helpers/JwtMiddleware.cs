using FetchApiTutorial.Services.UserService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FetchApiTutorial.Helpers.Settings;
using FetchApiTutorial.Services.JwtUtils;


namespace FetchApiTutorial.Helpers
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> appSettings)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserService userService, IJwtUtils jwtUtils)
        {
            var token = context.Request.Headers[JwtSettings.TokenKey].ToString().Split(" ").Last();

            var userId = jwtUtils.ValidateJwtToken(token);
            if (userId != null)
                context.Items[JwtSettings.UserKey] = await userService.GetByIdAsync(userId);

            await _next(context);
        }
    }
}
