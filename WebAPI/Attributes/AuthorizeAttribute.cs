using Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            User user = (User)context.HttpContext.Items["user"];

            if(user == null)
            {
                APIResponse response = new APIResponse()
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Message = "Not authorized",
                    Data = "You need to authenticate yourself to have access on this path"
                };

                context.Result = new JsonResult(response)
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
        }
    }
}
