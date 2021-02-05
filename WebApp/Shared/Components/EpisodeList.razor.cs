using Commons;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class EpisodeList
    {
        [Inject] protected IJSRuntime Js { get; set; }
        //[Inject] protected NavigationManager NavigationManager { get; set; }

        [Parameter] public List<Episodio> Episodi { get; set; }
        //[Parameter] public int AnimeId { get; set; }
        [Parameter] public int NumeroEpisodio { get; set; }
        [Parameter] public Action<int> CambiaEpisodio { get; set; }
        [Parameter] public RenderFragment Placeholder { get; set; }
        [Parameter] public string TrailerUrl { get; set; }

        private int NumeroGruppi { get; set; }

        protected override void OnInitialized()
        {
            NumeroGruppi = (int)Math.Floor(Episodi.Count / (double)100);
            base.OnInitialized();
        }

        private void ChangeEpisode(int Ep)
        {
            CambiaEpisodio?.Invoke(Ep);
        }
    }
}
