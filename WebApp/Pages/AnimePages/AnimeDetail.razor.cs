using Blazored.LocalStorage;
using Commons;
using Commons.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApp.Shared.Components;

namespace WebApp.Pages.AnimePages
{
    public partial class AnimeDetail
    {
        #region Injection
        [Inject] protected Generic Generic { get; set; }
        [Inject] protected HttpClient Client { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected SpinnerService Spinner { get; set; }
        [Inject] protected UIConfiguration Confs { get; set; }
        [Inject] protected IJSRuntime Js { get; set; }
        #endregion

        #region Parameter
        [Parameter] public int AnimeID { get; set; }
        [Parameter] public int NumeroEpisodio { get; set; }

        private Anime anime = new Commons.Anime();
        private Anime prequel;
        private Anime sequel;
        private AnimeEpisodi listEpisodi = new AnimeEpisodi() { Episodi = new List<Episodio>() };
        private string Localization;
        Uri TrailerUrl = new Uri("https://www.youtube-nocookie.com/embed/1fHQyRp2RGM");
        Dictionary<string, string> Episodi_Sources;
    #endregion

    #region life-cycle Page
    protected override void OnParametersSet()
        {
            Localization = LocalizationEnum.English;

            base.OnParametersSet();
        }

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine($"OnInitializedAsync - Numero Episodio: {NumeroEpisodio}");
            Spinner.Show();
            //await Task.Delay(5000);

            APIResponse ApiResponse = new APIResponse();

            //    //ApiResponse = await client.GetFromJsonAsync<APIResponse>("/api/v1/anime/" + AnimeID, jso);
            //    ApiResponse = await client.GetFromJsonAsync<APIResponse>("/api/v1/anime/MockUpResponse");
            //    ApiResponse = generic.GetSingleRequest<APIResponse>("/api/v1/anime/MockUpResponse");
            ApiResponse = GetAnime();
            anime = (Anime)ApiResponse.Data;

            if (ApiResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                NavigationManager.NavigateTo("/");
                return;
            }

            //anime = JsonSerializer.Deserialize<Anime>(ApiResponse.Data.ToString());


            if (anime.Prequel != null && anime.Prequel.HasValue)
            {
                ApiResponse = null;
                //ApiResponse = await client.GetFromJsonAsync<APIResponse>("/api/v1/anime/" + anime.Prequel);
                //ApiResponse = await Client.GetFromJsonAsync<APIResponse>("/api/v1/anime/MockUpResponse");
                //if (ApiResponse.StatusCode == System.Net.HttpStatusCode.OK)
                ApiResponse = GetAnime();
                prequel = (Anime)ApiResponse.Data;
            }

            if (anime.Sequel != null && anime.Sequel.HasValue)
            {
                ApiResponse = null;
                //ApiResponse = await client.GetFromJsonAsync<APIResponse>("/api/v1/anime/" + anime.Sequel);
                //ApiResponse = await Client.GetFromJsonAsync<APIResponse>("/api/v1/anime/MockUpResponse");
                //if (ApiResponse.StatusCode == System.Net.HttpStatusCode.OK)
                ApiResponse = GetAnime();
                sequel = (Anime)ApiResponse.Data;
            }

            try
            {
                //////TODO - Estrarre Episodi Anime
                ////listEpisodi = await client.GetFromJsonAsync<AnimeEpisodi>("/api/v1/episodi/" + AnimeID);
                listEpisodi = GetAnimeEpisodi();
                Episodi_Sources = new Dictionary<string, string>() { { "AnimeUnity", "AnimeUnity" }, { "Dreamsub", "Dreamsub" }, { "boh", "boh" } };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Generic Exception Catch: {ex.Message}");
                NavigationManager.NavigateTo("/");
                return;
            }

            Spinner.Hide();
        }

        #endregion

        #region Methods
        private void CambiaEpisodio(int _numEpisodio)
        {
            NavigationManager.NavigateTo($"/AnimeDetail/{AnimeID}/Episodio/{_numEpisodio}");
        }

        private void CambioSorgente(string Source)
        {
        }

        private APIResponse GetAnime()
        {
            Anime animeMK = new Anime();
            animeMK.Format = AnimeFormatEnum.TV;
            animeMK.EpisodesCount = 360;
            animeMK.EpisodeDuration = 23;
            animeMK.Status = AnimeStatusEnum.FINISHED;
            animeMK.StartDate = DateTime.Now;
            animeMK.SeasonPeriod = AnimeSeasonEnum.SUMMER;
            animeMK.TrailerUrl = "https://www.youtube.com/watch?v=1fHQyRp2RGM&ab_channel=Ronin97";

            try
            {
                //https://www.youtube.com/watch?v=1fHQyRp2RGM&ab_channel=Ronin97
                TrailerUrl = new Uri(animeMK.TrailerUrl);

                if (TrailerUrl != null)
                {
                    const string pattern = @"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)";
                    const string replacement = "https://www.youtube.com/embed/$1";

                    var rgx = new Regex(pattern);

                    if (TrailerUrl.Host.Contains("youtube"))
                        TrailerUrl = new(rgx.Replace(TrailerUrl.ToString(), replacement));
                }
            }
            catch (Exception)
            {
            }

            animeMK.SeasonYear = 2018;
            animeMK.Score = 79;
            animeMK.Genres = new List<string>() { "Action", "Drama", "Fantasy", "Mystery", "che ne so!!" };
            animeMK.CoverImage = "https://s4.anilist.co/file/anilistcdn/media/anime/cover/medium/bx104578-LaZYFkmhinfB.jpg";
            animeMK.BannerImage = "https://s4.anilist.co/file/anilistcdn/media/anime/banner/104578-z7SadpYEuAsy.jpg";
            animeMK.Sequel = 2;
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
        #endregion
    }
}
