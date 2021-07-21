using Microsoft.AspNetCore.Mvc.ApplicationModels;
using ServiceMongo.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Attributes;

namespace WebAPI
{
    public class CommaSeparatedQueryStringConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            foreach (var parameter in action.Parameters)
            {
                var props = parameter.ParameterType.GetProperties().Where(x => x.GetCustomAttributes(typeof(CommaSeparatedAttribute), false).Count() > 0);

                if(props.Count() > 0)
                {
                    var attribute = new SeparatedQueryStringAttribute(",");

                    parameter.Action.Filters.Add(attribute);
                    break;
                }
                //if (parameter.Attributes.OfType<CommaSeparatedAttribute>().Any() && !parameter.Action.Filters.OfType<SeparatedQueryStringAttribute>().Any())
                //{
                //    parameter.Action.Filters.Add(new SeparatedQueryStringAttribute(parameter.ParameterName, ","));
                //}
            }
        }
    }
}
