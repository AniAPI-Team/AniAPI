using Commons.Enums;
using GUI.Admin.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GUI.Admin.Controllers
{
    public class PageController : Controller
    {
        private readonly ILogger<PageController> _logger;

        public PageController(ILogger<PageController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        [Route("/Dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [Authorize]
        [Route("/Data/Anime")]
        public IActionResult Anime()
        {
            return View();
        }

        [Authorize]
        [Route("/Data/Anime/{animeID}")]
        public IActionResult Detail(long animeID)
        {
            return View(animeID);
        }

        [Authorize]
        [Route("/Stats/Digitalocean")]
        public IActionResult Digitalocean()
        {
            if(((ClaimsIdentity)User.Identity).GetSpecificClaim(ClaimTypes.Role) != UserRoleEnum.ADMINISTRATOR.ToString())
            {
                return Forbid();
            }

            return View();
        }

        [Authorize]
        [Route("/Stats/Cloudflare")]
        public IActionResult Cloudflare()
        {
            if (((ClaimsIdentity)User.Identity).GetSpecificClaim(ClaimTypes.Role) != UserRoleEnum.ADMINISTRATOR.ToString())
            {
                return Forbid();
            }

            return View();
        }

        [Authorize]
        [Route("/Stats/Webshare")]
        public IActionResult Webshare()
        {
            if (((ClaimsIdentity)User.Identity).GetSpecificClaim(ClaimTypes.Role) != UserRoleEnum.ADMINISTRATOR.ToString())
            {
                return Forbid();
            }

            return View();
        }
    }
}
