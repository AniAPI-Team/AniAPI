using Commons;
using Commons.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GUI.Admin.Controllers
{
    public class WebsiteController : Controller
    {
        private readonly ILogger<WebsiteController> _logger;

        private WebsiteCollection _websiteCollection;

        public WebsiteController(ILogger<WebsiteController> logger)
        {
            _logger = logger;

            _websiteCollection = new WebsiteCollection();
        }

        [Authorize]
        [HttpGet]
        public List<Website> GetWebsites()
        {
            try
            {
                return _websiteCollection.Collection.Find(Builders<Website>.Filter.Empty).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return null;
        }
    }
}
