using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class Tags
    {
        [Parameter] public List<string> TagList { get; set; }
    }
}
