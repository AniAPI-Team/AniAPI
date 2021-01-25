using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AniApi.Client
{
    public class UIConfigurations
    {
        public UIConfigurations(bool _UseCustomVideoPlayer)
        {
            UseCustomVideoPlayer = _UseCustomVideoPlayer;
        }

        public bool UseCustomVideoPlayer { get; set; }
    }
}
