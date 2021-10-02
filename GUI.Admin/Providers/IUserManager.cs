using Commons;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GUI.Admin.Providers
{
    public interface IUserManager
    {
        Task SignIn(HttpContext httpContext, User user, bool isPersistent = false);
        Task SignOut(HttpContext httpContext);
    }
}
