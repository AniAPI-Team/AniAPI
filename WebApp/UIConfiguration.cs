using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public class UIConfiguration
    {
        public UIConfiguration(bool useCustomVideoPlayer, bool useDarkTheme)
        {
            this.UseCustomVideoPlayer = useCustomVideoPlayer;
            this.UseDarkTheme = useDarkTheme;
        }

        public bool UseCustomVideoPlayer { get; set; }
        public bool UseDarkTheme { get; set; }
        public string DefaultGlobalization{ get; set; }
    }
}
