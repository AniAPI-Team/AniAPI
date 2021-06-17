using Commons;
using Commons.Collections;
using Commons.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiVersion("1")]
    [Route("oauth_client")]
    [ApiController]
    public class OAuthClientController : Controller
    {
        private readonly ILogger<OAuthClientController> _logger;
        private OAuthClientCollection _oAuthClientCollection = new OAuthClientCollection();

        public OAuthClientController(ILogger<OAuthClientController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}"), MapToApiVersion("1")]
        public APIResponse GetOne(long id)
        {
            try
            {
                OAuthClient client = this._oAuthClientCollection.Get(id);

                if (client == null)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Client not found",
                        $"Client with id {id} does not exists");
                }

                client.ClientSecret = Guid.Empty;

                return APIManager.SuccessResponse("Client found", client);
            }
            catch (APIException ex)
            {
                return APIManager.ErrorResponse(ex);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return APIManager.ErrorResponse();
            }
        }

        [HttpGet, MapToApiVersion("1")]
        public APIResponse GetMore([FromQuery] OAuthClientFilter filter)
        {
            throw new NotImplementedException();
        }

        [Authorize]
        [HttpPut, MapToApiVersion("1")]
        public APIResponse Create([FromBody] OAuthClient model)
        {
            try
            {
                User authenticatedUser = (User)HttpContext.Items["user"];

                if (string.IsNullOrEmpty(model.Name))
                {
                    throw new APIException(HttpStatusCode.BadRequest,
                        "Missing name",
                        "Please provide a valid name");
                }

                model.UserID = authenticatedUser.Id;
                model.ClientSecret = Guid.NewGuid();

                this._oAuthClientCollection.Add(ref model);

                return APIManager.SuccessResponse("OAuth client created", model);
            }
            catch (APIException ex)
            {
                return APIManager.ErrorResponse(ex);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return APIManager.ErrorResponse();
            }
        }

        [Authorize]
        [HttpPost, MapToApiVersion("1")]
        public APIResponse Update([FromBody] OAuthClient model)
        {
            throw new NotImplementedException();
        }

        [Authorize]
        [HttpDelete("{id}"), MapToApiVersion("1")]
        public APIResponse Delete(long id)
        {
            throw new NotImplementedException();
        }
    }
}
