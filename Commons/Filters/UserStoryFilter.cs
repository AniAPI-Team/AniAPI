using Commons.Enums;
using MongoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Filters
{
    public class UserStoryFilter : IFilter<UserStoryFilter>
    {
        public long? anime_id { get; set; }
        public long? user_id { get; set; }
        public UserStoryStatusEnum? status { get; set; }
        public bool? synced { get; set; }
    }
}
