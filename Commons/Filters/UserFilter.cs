using Microsoft.AspNetCore.Mvc;
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
        [FromQuery(Name = "username")]
        public string Username { get; set; }

        [FromQuery(Name = "email")]
        public string Email { get; set; }
    }
}
