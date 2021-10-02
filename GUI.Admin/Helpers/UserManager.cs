using Commons;
using GUI.Admin.Providers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GUI.Admin.Helpers
{
    public class UserManager : IUserManager
    {
        public async Task SignIn(HttpContext httpContext, User user, bool isPersistent = false)
        {
            var claims = getUserClaims(user);

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                IssuedUtc = new DateTimeOffset(DateTime.Now)
            };

            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
        }

        public async Task SignOut(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        List<Claim> getUserClaims(User user)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Username));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));
            claims.Add(new Claim(ClaimTypes.Gender, user.Gender.ToString()));
            claims.Add(new Claim("Avatar", user.Avatar));
            claims.Add(new Claim("Token", user.Token));

            return claims;
        }
    }
}
