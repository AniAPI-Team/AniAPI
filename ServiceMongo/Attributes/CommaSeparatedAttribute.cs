using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceMongo.Attributes
{
    
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class CommaSeparatedAttribute : Attribute
    {
    }
}
