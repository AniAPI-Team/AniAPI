using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Middlewares
{
    public interface IRateLimitMiddleware
    {
        public Task InvokeAsync(HttpContext httpContext);
    }
}
