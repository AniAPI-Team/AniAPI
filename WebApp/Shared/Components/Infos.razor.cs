using Commons;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class Infos
    {
        [Parameter] public Anime Anime { get; set; }
        [Parameter] public RenderFragment Placeholder { get; set; }

        protected override bool ShouldRender() => false;
    }
}
