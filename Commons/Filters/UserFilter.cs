using MongoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Filters
{
    public class UserFilter : IFilter<UserFilter>
    {
        public string username { get; set; }
        public string email { get; set; }
    }
}
