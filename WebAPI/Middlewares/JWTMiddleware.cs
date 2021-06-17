using Commons;
using Commons.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Middlewares
{
    public class JWTMiddleware
    {
        private UserCollection _userCollection = new UserCollection();
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public JWTMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(' ').Last();

            if(token != null)
            {
                try
                {
                    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                    byte[] key = Encoding.ASCII.GetBytes((string)_configuration.GetValue(typeof(string), "jwt_secret"));

                    tokenHandler.ValidateToken(token, new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
                    long userId = long.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                    User user = _userCollection.Get(userId);

                    if (!user.EmailVerified.Value)
                    {
                        httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                        APIResponse response = new APIResponse()
                        {
                            StatusCode = HttpStatusCode.Unauthorized,
                            Message = "Unauthorized",
                            Data = "You need to verify your email first"
                        };

                        await httpContext.Response.WriteAsJsonAsync(response);
                        return;
                    }

                    httpContext.Items["user"] = user;
                }
                catch {}
            }

            await _next(httpContext);
        }
    }

    public static class JWTMiddlewareExtensions
    {
        public static IApplicationBuilder UseJWTMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JWTMiddleware>();
        }
    }
}
