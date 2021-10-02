using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GUI.Admin.Helpers
{
    public static class HtmlHelper
    {
        public static string IsSidebarItemActive(this IHtmlHelper htmlHelper, string controllers, string actions, string cssClass = "active")
        {
            string action = (string)htmlHelper.ViewContext.RouteData.Values["action"];
            string controller = (string)htmlHelper.ViewContext.RouteData.Values["controller"];

            IEnumerable<string> acceptedActions = (actions ?? action).Split(',');
            IEnumerable<string> acceptedController = (controllers ?? controller).Split(',');

            return acceptedActions.Contains(action) && acceptedController.Contains(controller) ? cssClass : string.Empty;
        }

        public static string GetSpecificClaim(this ClaimsIdentity claimsIdentity, string claimType)
        {
            var claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == claimType);

            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}
