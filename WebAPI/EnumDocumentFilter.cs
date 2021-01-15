using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI
{
    public class EnumDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach(var property in swaggerDoc.Components.Schemas.Where(x => x.Value?.Enum?.Count > 0))
            {
                IList<IOpenApiAny> enums = property.Value.Enum;
                if(enums != null && enums.Count > 0)
                {
                    property.Value.Description += DescribeEnum(enums, property.Key);
                }
            }

            foreach (var item in swaggerDoc.Paths.Values)
            {
                DescribeEnumParameters(item.Operations, swaggerDoc);
            }
        }

        private void DescribeEnumParameters(IDictionary<OperationType, OpenApiOperation> operations, OpenApiDocument swaggerDoc)
        {
            if(operations != null)
            {
                foreach(var op in operations)
                {
                    foreach(var p in op.Value.Parameters)
                    {
                        var pEnum = swaggerDoc.Components.Schemas.FirstOrDefault(x => x.Key == p.Name);
                        if(pEnum.Value != null)
                        {
                            p.Description += DescribeEnum(pEnum.Value.Enum, pEnum.Key);
                        }
                    }
                }
            }
        }

        private Type GetEnumTypeByName(string enumTypeName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.Name == enumTypeName);
        }

        private string DescribeEnum(IList<IOpenApiAny> enums, string pName)
        {
            List<string> descriptions = new List<string>();
            var enumType = GetEnumTypeByName(pName);

            if(enumType == null)
            {
                return null;
            }

            foreach(OpenApiInteger enumOption in enums)
            {
                int enumInt = enumOption.Value;
                descriptions.Add(string.Format("{0} = {1}", enumInt, Enum.GetName(enumType, enumInt)));
            }

            return string.Join(", ", descriptions.ToArray());
        }
    }
}
