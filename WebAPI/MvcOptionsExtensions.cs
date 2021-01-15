using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI
{
    public static class MvcOptionsExtensions
    {
        public static void UseGeneralRoutePrefix(this MvcOptions options, IRouteTemplateProvider routeTemplate)
        {
            options.Conventions.Add(new RoutePrefixConvention(routeTemplate));
        }

        public static void UseGeneralRoutePrefix(this MvcOptions options, string prefix)
        {
            options.UseGeneralRoutePrefix(new RouteAttribute(prefix));
        }
    }

    public class RoutePrefixConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _routeModel;

        public RoutePrefixConvention(IRouteTemplateProvider routeTemplate)
        {
            this._routeModel = new AttributeRouteModel(routeTemplate);
        }

        public void Apply(ApplicationModel application)
        {
            foreach(var selector in application.Controllers.SelectMany(x => x.Selectors))
            {
                if(selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(this._routeModel, selector.AttributeRouteModel);
                }
                else
                {
                    selector.AttributeRouteModel = this._routeModel;
                }
            }
        }
    }
}
