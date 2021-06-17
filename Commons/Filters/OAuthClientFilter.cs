using MongoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Filters
{
    public class OAuthClientFilter : IFilter<OAuthClientFilter>
    {
        public long? user_id { get; set; }
        public Guid? client_id { get; set; }
    }
}
