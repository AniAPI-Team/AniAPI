using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared
{
    public partial class LoginLayout
    {
        [Inject] protected UIConfiguration Theme { get; set; }
    }
}
