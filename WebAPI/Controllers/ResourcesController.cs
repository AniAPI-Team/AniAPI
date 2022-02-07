using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for internal resources
    /// </summary>
    [ApiVersion("1")]
    [Route("resources")]
    [ApiController]
    public class ResourcesController : Controller
    {
        private readonly ILogger<ResourcesController> _logger;
        private AppSettingsCollection _appSettingsCollection = new AppSettingsCollection();
        private Assembly _assembly;

        public ResourcesController(ILogger<ResourcesController> logger)
        {
            _logger = logger;
            _assembly = Assembly.GetEntryAssembly();
        }

        /// <summary>
        /// Retrieves the resources version
        /// </summary>
        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet, MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public APIResponse GetVersion()
        {
            try
            {
                IEnumerable<KeyValuePair<string, string>> settings = this._appSettingsCollection.GetConfiguration();

                return APIManager.SuccessResponse("Resources version", settings.FirstOrDefault(x => x.Key == "resources_version").Value);
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
        /// Retrieves a resource version
        /// </summary>
        /// <param name="resource_type">The resource type</param>
        /// <param name="resource_version">The resource version</param>
        [AllowAnonymous]
        [EnableCors("CorsEveryone")]
        [HttpGet("{resource_version}/{resource_type}"), MapToApiVersion("1")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public APIResponse GetResource(ResourceTypeEnum resource_type, string resource_version = "1.0")
        {
            try
            {
                if(this._assembly == null)
                {
                    throw new Exception();
                }

                string resourceName = string.Empty;

                switch (resource_type)
                {
                    case ResourceTypeEnum.GENRES:
                        resourceName = "genres";
                        break;
                    case ResourceTypeEnum.LOCALIZATIONS:
                        resourceName = "localizations";
                        break;
                }

                EmbeddedFileProvider provider = new EmbeddedFileProvider(this._assembly);
                IFileInfo resourceInfo = provider.GetFileInfo($"Resources.{resourceName}.{resource_version.Replace('.', '_')}.json");

                if(!resourceInfo.Exists)
                {
                    throw new APIException(HttpStatusCode.NotFound,
                        "Resource not found",
                        $"Resource of type {resourceName} does not exists");
                }

                Stream resourceStream = resourceInfo.CreateReadStream();

                object resourceContent = null;
                using(StreamReader reader = new StreamReader(resourceStream, Encoding.UTF8))
                {
                    switch (resource_type)
                    {
                        case ResourceTypeEnum.GENRES:
                            resourceContent = JsonConvert.DeserializeObject<GenresResource>(reader.ReadToEnd());
                            break;
                        case ResourceTypeEnum.LOCALIZATIONS:
                            resourceContent = JsonConvert.DeserializeObject<LocalizationResource>(reader.ReadToEnd());
                            break;
                    }
                }

                return APIManager.SuccessResponse("Resource found", resourceContent);
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
