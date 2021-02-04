using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class YoutubePlayer
    {
        [Inject] protected IJSRuntime Js { get; set; }
        [Parameter] public Uri TrailerUrl { get; set; }
    }
}
