using Commons;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class VideoPlayer : BaseComponent
    {
        [Parameter] public AnimeEpisodi listEpisodi { get; set; }
        [Parameter] public int VideoNumeroEpisodio { get; set; }
        [Parameter] public Action<int> VideoNumeroEpisodioChanged { get; set; }

        public Episodio episodioAttuale = new Episodio();

        protected override void OnParametersSet()
        {
            if (listEpisodi == null)
                return;

            if (listEpisodi.Episodi != null)
            {

                episodioAttuale = listEpisodi.Episodi.Where(num => num.NumeroEpisodio == VideoNumeroEpisodio).FirstOrDefault();

                if (episodioAttuale != null && episodioAttuale.Url != null)
                {
                    JS.InvokeVoidAsync("VideoPlayer.ChangeVideoUrl", episodioAttuale.Url);
                }

                //StateHasChanged();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.InvokeVoidAsync("VideoPlayer.InitVideoVariables");
            }

            if (!firstRender)
            {
                await JS.InvokeVoidAsync("VideoPlayer.ScroolToVideo");
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public void NextEpisode()
        {
            if (VideoNumeroEpisodio + 1 <= listEpisodi.Episodi.Count)
            {
                VideoNumeroEpisodio++;
                VideoNumeroEpisodioChanged?.Invoke(VideoNumeroEpisodio);
            }
        }

        public async Task ToggleFullScreen()
        {
            await JS.InvokeVoidAsync("VideoPlayer.ToggleFullScreen");
        }

        public async Task TogglePlay()
        {
            await JS.InvokeVoidAsync("VideoPlayer.TogglePlay");
        }
    }
}
