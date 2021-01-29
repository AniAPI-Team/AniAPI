using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Middlewares
{
    public interface IRateLimitDependency
    {
        APIRequestIP CanRequest(string ip);
    }
}
