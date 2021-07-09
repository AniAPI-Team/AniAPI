using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    /// <summary>
    /// CRUD Controller for OAuthClient resource
    /// </summary>
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

        /// <summary>
        /// Retrieves a specific OAuthClient by id
        /// </summary>
        /// <param name="id">The OAuthClient id</param>
        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
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

        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet, MapToApiVersion("1")]
        public APIResponse GetMore([FromQuery] OAuthClientFilter filter)
        {
            try
            {
                Paging<OAuthClient> result = this._oAuthClientCollection.GetList<OAuthClientFilter>(filter);

                if (result.LastPage == 0)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Zero clients found",
                        "");
                }

                if (filter.page > result.LastPage)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Page out of range",
                        $"Last page number available is {result.LastPage}");
                }

                foreach (OAuthClient c in result.Documents)
                {
                    c.ClientSecret = Guid.Empty;
                }

                return APIManager.SuccessResponse($"Page {result.CurrentPage} contains {result.Documents.Count} clients. Last page number is {result.LastPage} for a total of {result.Count} clients", result);
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

        /// <summary>
        /// Create a new OAuthClient
        /// </summary>
        /// <param name="model">The OAuthClient model</param>
        [Authorize]
#if DEBUG
        [EnableCors("CorsEveryone")]
#else
        [EnableCors("CorsInternal")]
#endif
        [HttpPut, MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
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

        /// <summary>
        /// Update an existing OAuthClient
        /// </summary>
        /// <param name="model">The OAuthClient model</param>
        [Authorize]
#if DEBUG
        [EnableCors("CorsEveryone")]
#else
        [EnableCors("CorsInternal")]
#endif
        [HttpPost, MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public APIResponse Update([FromBody] OAuthClient model)
        {
            try
            {
                User authenticatedUser = (User)HttpContext.Items["user"];

                if (authenticatedUser.Role == UserRoleEnum.BASIC && authenticatedUser.Id != model.UserID)
                {
                    throw new APIException(HttpStatusCode.Forbidden,
                        "Forbidden",
                        "You have no access rights to edit this client");
                }

                if (!this._oAuthClientCollection.Exists(ref model, false))
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Client not found",
                        $"Client with id {model.Id} does not exists");
                }

                OAuthClient client = this._oAuthClientCollection.Get(model.Id);
                client.Name = model.Name;
                client.RedirectURI = model.RedirectURI;

                this._oAuthClientCollection.Edit(ref client);

                return APIManager.SuccessResponse("Client updated", client);
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

        /// <summary>
        /// Delete an existing OAuthClient by id
        /// </summary>
        /// <param name="id">The OAuthClient id</param>
        [Authorize]
#if DEBUG
        [EnableCors("CorsEveryone")]
#else
        [EnableCors("CorsInternal")]
#endif
        [HttpDelete("{id}"), MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public APIResponse Delete(long id)
        {
            try
            {
                User authenticatedUser = (User)HttpContext.Items["user"];

                OAuthClient client = this._oAuthClientCollection.Get(id);

                if(client == null)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Client not found",
                        $"Client with id {id} does not exists");
                }

                if (authenticatedUser.Role == UserRoleEnum.BASIC && authenticatedUser.Id != client.UserID)
                {
                    throw new APIException(HttpStatusCode.Forbidden,
                        "Forbidden",
                        "You have no access rights to delete this client");
                }

                this._oAuthClientCollection.Delete(id);

                return APIManager.SuccessResponse("Client deleted");
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
    }
}
