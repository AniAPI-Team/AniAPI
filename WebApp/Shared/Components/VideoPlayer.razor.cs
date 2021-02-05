using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Commons;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class VideoPlayer
    {
        #region Injection

        [Inject] protected IJSRuntime Js { get; set; }

        #endregion

        [Parameter] public AnimeEpisodi listEpisodi { get; set; }
        [Parameter] public int VideoNumeroEpisodio { get; set; }
        [Parameter] public Action<int> CambiaEpisodio { get; set; }
        [Parameter] public RenderFragment Placeholder { get; set; }
        [Parameter] public string TrailerUrl { get; set; }

        private bool NeedRender = false;
        public Episodio episodioAttuale = new Episodio();

        protected override bool ShouldRender() => NeedRender;

        protected override void OnParametersSet()
        {
            NeedRender = false;

            if (listEpisodi.Episodi != null)
            {
                episodioAttuale = listEpisodi.Episodi.Where(num => num.NumeroEpisodio == VideoNumeroEpisodio && num.Url != null).FirstOrDefault();

                if (episodioAttuale != null)
                {
                    Js.InvokeVoidAsync("VideoPlayer.ChangeVideoUrl", episodioAttuale.Url);
                    NeedRender = true;
                }

                if (VideoNumeroEpisodio == 0)
                {
                    Js.InvokeVoidAsync("VideoPlayer.ChangeVideoUrl", TrailerUrl);
                    NeedRender = true;
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await Js.InvokeVoidAsync("VideoPlayer.InitVideoVariables");

            if (!firstRender || TrailerUrl != null)
            {
                await Js.InvokeVoidAsync("VideoPlayer.ScroolToVideo");
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public void NextEpisode()
        {
            if (VideoNumeroEpisodio + 1 <= listEpisodi.Episodi.Count)
                CambiaEpisodio?.Invoke(VideoNumeroEpisodio +1 );
        }

        public async Task ToggleFullScreen()
        {
            await Js.InvokeVoidAsync("VideoPlayer.ToggleFullScreen");
        }

        public async Task TogglePlay()
        {
            await Js.InvokeVoidAsync("VideoPlayer.TogglePlay");
        }
    }
}
