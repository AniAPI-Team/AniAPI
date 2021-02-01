using Commons;
using Commons.Enums;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WebApp.Shared.Components;

namespace WebApp.Pages.AnimePages
{
    public partial class AnimeDetail : BaseComponent
    {
        [Parameter] public int AnimeID { get; set; }
        [Parameter] public int NumeroEpisodio { get; set; }

        public VideoPlayer vp = new VideoPlayer();
        //public SpinnerService spinner;
        //public NavigationManager navigationManager;
        //public HttpClient client;

        public Anime anime = new Commons.Anime();
        public Anime prequel;
        public Anime sequel;
        public AnimeEpisodi listEpisodi = new AnimeEpisodi();

        protected override void OnParametersSet()
        {
            if (NumeroEpisodio == 0)
                NumeroEpisodio = -1;

            base.OnParametersSet();
        }

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine($"OnInitializedAsync - Numero Episodio: {NumeroEpisodio}");
            spinner.Show();
            //await Task.Delay(5000);

            APIResponse ApiResponse = new APIResponse();

            //    //ApiResponse = await client.GetFromJsonAsync<APIResponse>("/api/v1/anime/" + AnimeID, jso);
            //    ApiResponse = await client.GetFromJsonAsync<APIResponse>("/api/v1/anime/MockUpResponse");
            //    ApiResponse = generic.GetSingleRequest<APIResponse>("/api/v1/anime/MockUpResponse");
            ApiResponse = GetAnime();
            anime = (Anime)ApiResponse.Data;

            if (ApiResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                navigationManager.NavigateTo("/");
                return;
            }

            //anime = JsonSerializer.Deserialize<Anime>(ApiResponse.Data.ToString());


            if (anime.Prequel != null && anime.Prequel.HasValue)
            {
                ApiResponse = null;
                //ApiResponse = await client.GetFromJsonAsync<APIResponse>("/api/v1/anime/" + anime.Prequel);
                ApiResponse = await client.GetFromJsonAsync<APIResponse>("/api/v1/anime/MockUpResponse");
                if (ApiResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    prequel = (Anime)ApiResponse.Data;
            }

            if (anime.Sequel != null && anime.Sequel.HasValue)
            {
                ApiResponse = null;
                //ApiResponse = await client.GetFromJsonAsync<APIResponse>("/api/v1/anime/" + anime.Sequel);
                ApiResponse = await client.GetFromJsonAsync<APIResponse>("/api/v1/anime/MockUpResponse");
                if (ApiResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    sequel = (Anime)ApiResponse.Data;
            }

            try
            {
                //////TODO - Estrarre Episodi Anime
                ////listEpisodi = await client.GetFromJsonAsync<AnimeEpisodi>("/api/v1/episodi/" + AnimeID);
                listEpisodi = GetAnimeEpisodi();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Generic Exception Catch: {ex.Message}");
                navigationManager.NavigateTo("/");
                return;
            }

            spinner.Hide();
        }

        public void ChildFiredEvent(int _numEpisodio)
        {
            NumeroEpisodio = _numEpisodio;
            //StateHasChanged();
            InvokeAsync(StateHasChanged);
        }

        private APIResponse GetAnime()
        {
            Anime animeMK = new Anime();
            animeMK.Format = AnimeFormatEnum.TV;
            animeMK.EpisodesCount = 35;
            animeMK.EpisodeDuration = 23;
            animeMK.Status = AnimeStatusEnum.FINISHED;
            animeMK.StartDate = DateTime.Now;
            animeMK.SeasonPeriod = AnimeSeasonEnum.SUMMER;
            animeMK.SeasonYear = 2018;
            animeMK.Score = 79;
            animeMK.Genres = new List<string>() { "Action", "Drama", "Fantasy", "Mystery", "che ne so!!" };
            animeMK.CoverImage = "https://s4.anilist.co/file/anilistcdn/media/anime/cover/medium/bx104578-LaZYFkmhinfB.jpg";
            animeMK.BannerImage = "https://s4.anilist.co/file/anilistcdn/media/anime/banner/104578-z7SadpYEuAsy.jpg";
            //animeMK.Sequel = 2;
            //animeMK.Prequel = 2;
            animeMK.TrailerUrl = "https://www.animeunityserver75.cloud/DDL/Anime/OnePieceITA/OnePiece_Ep_001_ITA.mp4";


            animeMK.Titles = new Dictionary<string, string>();
            animeMK.Titles.Add(LocalizationEnum.English, "Shingeki no Kyojin 3 Part 2");

            animeMK.Descriptions = new Dictionary<string, string>();
            animeMK.Descriptions.Add(LocalizationEnum.English, "The second cour of <i>Shingeki no Kyojin 3</i>.<br><br>The battle to retake Wall Maria begins now! With Eren’s new hardening ability, the Scouts are confident they can seal the wall and take back Shiganshina District. If they succeed, Eren can finally unlock the secrets of the basement—and the world. But danger lies in wait as Reiner, Bertholdt, and the Beast Titan have plans of their own. Could this be humanity’s final battle for survival?<br><br>(Source: Funimation)");


            APIResponse response = new APIResponse();

            response.StatusCode = System.Net.HttpStatusCode.OK;
            response.Message = null;
            response.Data = animeMK;
            response.Version = "1.0.1.0";

            return response;
        }

        private AnimeEpisodi GetAnimeEpisodi()
        {
            AnimeEpisodi listEp = new AnimeEpisodi();
            listEp.Episodi = new List<Episodio>();
            for (int i = 1; i <= anime.EpisodesCount; i++)
            {
                string fmt = "000";
                string urlEp = $"https://www.animeunityserver75.cloud/DDL/Anime/OnePieceITA/OnePiece_Ep_{i.ToString(fmt)}_ITA.mp4";
                Episodio ep = new Episodio() { NumeroEpisodio = i, Url = urlEp, Sorgente = "animeunity" };
                listEp.Episodi.Add(ep);
            }

            return listEp;
        }

        //private Task CambioEpisodio(int NumEp)
        //{
        //    NumeroEpisodio = NumEp;
        //    return Task.CompletedTask;
        //}
    }
}
