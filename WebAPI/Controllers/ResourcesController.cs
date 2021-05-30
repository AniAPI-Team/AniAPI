using Commons;
using Commons.Collections;
using Commons.Resources;
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

        [HttpGet, MapToApiVersion("1")]
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

        [HttpGet("{resourceName}"), MapToApiVersion("1")]
        public APIResponse GetResource(string resourceName)
        {
            try
            {
                if(this._assembly == null)
                {
                    throw new Exception();
                }

                EmbeddedFileProvider provider = new EmbeddedFileProvider(this._assembly);
                IFileInfo resourceInfo = provider.GetFileInfo($"Resources.{resourceName}.json");

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
                    switch (resourceName)
                    {
                        case "genres":
                            resourceContent = JsonConvert.DeserializeObject<GenresResource>(reader.ReadToEnd());
                            break;
                        case "translations":
                            resourceContent = JsonConvert.DeserializeObject<TranslationsResource>(reader.ReadToEnd());
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
