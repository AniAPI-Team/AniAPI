using Commons;
using Commons.Collections;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace WebAPI.Helpers
{
    public class JWTAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private UserCollection _userCollection = new UserCollection();
        private IConfiguration _configuration;

        public JWTAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration) : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Endpoint? endpoint = Context.GetEndpoint();

            if(endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return AuthenticateResult.NoResult();
            }

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization header");
            }

            User user = null;

            try
            {
                AuthenticationHeaderValue auth = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                
                if(auth.Scheme != "Bearer")
                {
                    throw new Exception();
                }

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                byte[] key = Encoding.ASCII.GetBytes((string)_configuration.GetValue(typeof(string), "jwt_secret"));

                tokenHandler.ValidateToken(auth.Parameter, new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
                long userId = long.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                user = _userCollection.Get(userId);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization header");
            }

            if(user == null)
            {
                return AuthenticateResult.Fail("Invalid JWT token");
            }

            if (!user.EmailVerified.Value)
            {
                return AuthenticateResult.Fail("Email not verified");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Context.Items["user"] = user;

            return AuthenticateResult.Success(ticket);
        }
    }
}
