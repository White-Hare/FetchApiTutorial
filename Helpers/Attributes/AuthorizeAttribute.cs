using FetchApiTutorial.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using FetchApiTutorial.Helpers.Settings;


namespace FetchApiTutorial.Helpers.Attributes
{
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if(allowAnonymous)
                return;

            var user = (MyUser) context.HttpContext.Items[JwtSettings.UserKey];

            if (user == null)
                context.Result = new JsonResult(new {message = "Unauthorized"})
                    {StatusCode = StatusCodes.Status401Unauthorized};

        }
    }
}
