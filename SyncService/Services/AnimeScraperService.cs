using Commons;
using Commons.Collections;
using Newtonsoft.Json;
using SyncService.Models;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using Commons.Enums;
using System.Threading.Tasks;
using PuppeteerSharp;
using SpotifyAPI.Web;
using System.Web;
using System.Linq;

namespace SyncService.Services
{
    public class AnimeScraperService : IService
    {
        #region Members

        private AppSettings _appSettings;
        private AnimeCollection _animeCollection = new AnimeCollection();
        private HttpClient _anilistClient = new HttpClient() { BaseAddress = new Uri("https://graphql.anilist.co") };
        private GraphQLQuery _anilistQuery = new GraphQLQuery()
        {
            Query = @"
            query($page: Int, $format: MediaFormat) {
                Page(page: $page, perPage: 50) {
                    pageInfo {
                        lastPage
                    }
                    media(format: $format) {
                        id
                        idMal
                        format
                        status
                        title {
                            romaji
                            english
                            native
                        }
                        description
                        startDate {
                            year
                            month
                            day
                        }
                        endDate {
                            year
                            month
                            day
                        }
                        season
                        seasonYear
                        episodes
                        duration
                        trailer {
                            id
                            site
                        }
                        coverImage {
                            large
                            color
                        }
                        bannerImage
                        genres
                        tags {
                            name
                        }
                        relations {
                            edges {
                                relationType
                                node {
                                    id
                                    format
                                }
                            }
                        }
                        nextAiringEpisode {
                            episode
                        }
                        recommendations {
                            nodes {
                                rating
                                mediaRecommendation {
                                    id
                                }
                            }
                        }
                        averageScore
                    }
                }
            }
            ",
            Variables = new Dictionary<string, object>()
        };
        private int _totalPages = 1;
        private List<string> _formatsFilter = new List<string>() { "TV", "TV_SHORT", "MOVIE", "SPECIAL", "OVA", "ONA", "MUSIC" };
        private int _rateLimitRemaining;
        private long? _rateLimitReset;

        private HttpClient _tmdbClient = new HttpClient() { BaseAddress = new Uri("https://api.themoviedb.org/") };

        protected override int TimeToWait => 60 * 1000 * 60 * 12; // 12 Hours

        #endregion
        
        protected override ServicesStatus GetServiceStatus()
        {
            return new ServicesStatus("AnimeScraper");
        }

        public override async Task Start(CancellationToken cancellationToken)
        {
            this._appSettings = new AppSettingsCollection().Get(0);

            this._rateLimitRemaining = 90;
            this._rateLimitReset = DateTime.Now.Ticks;

            this._tmdbClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _appSettings.TmdbKey);
            
            await base.Start(cancellationToken);
        }

        public override async Task Work()
        {
            await base.Work();

            try
            {
                foreach (string formatFilter in this._formatsFilter)
                {
                    this._anilistQuery.Variables["format"] = formatFilter;

                    for (int currentPage = 1; currentPage <= this._totalPages; currentPage++)
                    {
                        this._anilistQuery.Variables["page"] = currentPage;

                        var request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            Content = new StringContent(JsonConvert.SerializeObject(this._anilistQuery), Encoding.UTF8, "application/json")
                        };

                        try
                        {
                            using (var response = await this._anilistClient.SendAsync(request))
                            {
                                try
                                {
                                    response.EnsureSuccessStatusCode();
                                }
                                catch (Exception ex)
                                {
                                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                                    {
                                        if (response.Headers.Contains("X-RateLimit-Reset"))
                                        {
                                            this._rateLimitReset = Convert.ToInt64(((string[])response.Headers.GetValues("X-RateLimit-Reset"))[0]);
                                        }
                                        else
                                        {
                                            this._rateLimitReset = null;
                                        }
                                    }

                                    throw new HttpRequestException("RateLimit superato", ex);
                                }

                                AnilistResponse anilistResponse = JsonConvert.DeserializeObject<AnilistResponse>(await response.Content.ReadAsStringAsync());

                                if (currentPage == 1)
                                {
                                    this._totalPages = anilistResponse.Data.Page.PageInfo.LastPage;
                                }

                                foreach (AnilistResponse.ResponseMedia m in anilistResponse.Data.Page.Media)
                                {
                                    Anime anime = new Anime(m);

                                    switch (formatFilter)
                                    {
                                        case "TV":
                                        case "TV_SHORT":
                                        case "SPECIAL":
                                        case "OVA":
                                        case "ONA":
                                            anime = await linkTvShowToTMDB(anime);

                                            if (anime.TmdbId.HasValue)
                                            {
                                                anime = await getTvShowTMBDInfo(anime);
                                            }
                                            break;
                                        case "MOVIE":
                                            anime = await linkMovieToTMDB(anime);

                                            if (anime.TmdbId.HasValue)
                                            {
                                                anime = await getMovieTMBDInfo(anime);
                                            }
                                            break;
                                    }

                                    if (this._animeCollection.Exists(ref anime))
                                    {
                                        this._animeCollection.Edit(ref anime);
                                    }
                                    else
                                    {
                                        this._animeCollection.Add(ref anime);
                                    }
                                }

                                this._rateLimitRemaining = Convert.ToInt32(((string[])response.Headers.GetValues("X-RateLimit-Remaining"))[0]);

#if DEBUG
                                this.Log($"Format {formatFilter} done {GetProgress(currentPage, this._totalPages)}%");
#endif
                                this.Log($"Format {formatFilter} done {GetProgress(currentPage, this._totalPages)}%", true);
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            currentPage--;

                            DateTime timeOfReset = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                            if (this._rateLimitReset.HasValue)
                            {
                                timeOfReset = timeOfReset.AddSeconds(this._rateLimitReset.Value).ToLocalTime();
                            }
                            else
                            {
                                timeOfReset = DateTime.Now.AddSeconds(120);
                            }

                            TimeSpan timeToWait = timeOfReset - DateTime.Now;

                            this.Log($"Waiting {timeToWait.TotalMilliseconds.ToString("F0")} ms!", true);

                            Thread.Sleep((int)timeToWait.TotalMilliseconds + 1000);
                        }

                        if (_cancellationToken.IsCancellationRequested)
                        {
                            throw new TaskCanceledException("Process cancellation requested!");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        async Task<Anime> linkTvShowToTMDB(Anime anime)
        {
            try
            {
                List<TmdbSearchTvResponse.ResponseResult> results = new List<TmdbSearchTvResponse.ResponseResult>();
                int lastPage = int.MaxValue;

                for(int page = 1; page < lastPage; page++)
                {
                    string url = $"/3/search/tv?query={HttpUtility.UrlEncode(anime.Titles[LocalizationEnum.Romaji])}&page={page}";
                    using (var response = await this._tmdbClient.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();

                        TmdbSearchTvResponse searchResponse = JsonConvert.DeserializeObject<TmdbSearchTvResponse>(await response.Content.ReadAsStringAsync());

                        if(lastPage == int.MaxValue)
                        {
                            lastPage = searchResponse.TotalPages;
                        }

                        results.AddRange(searchResponse.Results);
                    }
                }

                TmdbSearchTvResponse.ResponseResult match = null;
                bool exactMatch = false;

                foreach(var result in results)
                {
                    if(exactMatch == true)
                    {
                        continue;
                    }

                    List<TmdbAlternativeTitlesResponse.ResponseResult> titles = new List<TmdbAlternativeTitlesResponse.ResponseResult>();

                    string url = $"/3/tv/{result.Id}/alternative_titles";
                    using (var response = await this._tmdbClient.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();

                        TmdbAlternativeTitlesResponse titlesResponse = JsonConvert.DeserializeObject<TmdbAlternativeTitlesResponse>(await response.Content.ReadAsStringAsync());
                        titles = titlesResponse.Results;
                    }

                    string title = anime.Titles[LocalizationEnum.English] ?? anime.Titles[LocalizationEnum.Romaji];
                    List<string> matchTitles = new List<string> { result.Name };

                    foreach(var t in titles.Where(x => x.Iso3166 == "JP" || x.Iso3166 == "US"))
                    {
                        matchTitles.Add(t.Title);
                    }

                    foreach(string matchTitle in matchTitles)
                    {
                        if (title.ToLower() == matchTitle.ToLower())
                        {
                            if (anime.StartDate.HasValue &&
                                result.StartDate.HasValue)
                            {
                                if (anime.StartDate.Value.Year == result.StartDate.Value.Year)
                                {
                                    match = result;
                                    exactMatch = true;
                                    break;
                                }
                            }
                            else if (!exactMatch)
                            {
                                match = result;
                            }
                        }
                    }
                }

                if(match != null)
                {
                    anime.TmdbId = match.Id;
                }
            }
            catch(Exception ex)
            {
#if DEBUG
                this.Log(ex.Message);
#endif
            }
                
            return anime;
        }

        async Task<Anime> getTvShowTMBDInfo(Anime anime)
        {
            try
            {
                int seasons = 0;

                string url = $"/3/tv/{anime.TmdbId}";
                using (var response = await this._tmdbClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();

                    TmdbDetailTvResponse detailResponse = JsonConvert.DeserializeObject<TmdbDetailTvResponse>(await response.Content.ReadAsStringAsync());

                    if (detailResponse.LastAirDate.HasValue)
                    {
                        anime.WeeklyAiringDay = detailResponse.LastAirDate.Value.DayOfWeek;
                    }

                    if (detailResponse.NumberOfSeasons.HasValue)
                    {
                        seasons = detailResponse.NumberOfSeasons.Value;
                    }
                }

                url = $"/3/tv/{anime.TmdbId}/alternative_titles";
                using (var response = await this._tmdbClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();

                    TmdbAlternativeTitlesResponse titlesResponse = JsonConvert.DeserializeObject<TmdbAlternativeTitlesResponse>(await response.Content.ReadAsStringAsync());
                    
                    foreach(var t in titlesResponse.Results.GroupBy(x => x.Iso3166))
                    {
                        string locale = LocalizationEnum.FormatIsoToLocale(t.Key);

                        if(locale != LocalizationEnum.English && 
                            locale != LocalizationEnum.Japanese && 
                            LocalizationEnum.IsLocaleSupported(locale))
                        {
                            anime.Titles[locale] = t.FirstOrDefault().Title;
                        }
                    }
                }
       
                url = $"/3/tv/{anime.TmdbId}/translations";
                using (var response = await this._tmdbClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();

                    TmdbTranslationsResponse seasonResponse = JsonConvert.DeserializeObject<TmdbTranslationsResponse>(await response.Content.ReadAsStringAsync());

                    foreach (var t in seasonResponse.Translations)
                    {
                        string locale = LocalizationEnum.FormatIsoToLocale(t.Iso639);
                        if (locale != LocalizationEnum.English && LocalizationEnum.IsLocaleSupported(locale))
                        {
                            anime.Descriptions[locale] = t.Data.Overview;
                        }
                    }
                }

                anime.Sagas = new List<Anime.Saga>();

                for (int i = 1; i <= seasons; i++)
                {
                    Anime.Saga saga = new Anime.Saga
                    {
                        Titles = new Dictionary<string, string>(),
                        Descriptions = new Dictionary<string, string>()
                    };

                    url = $"/3/tv/{anime.TmdbId}/season/{i}";
                    using (var response = await this._tmdbClient.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();

                        TmdbSeasonTvResponse seasonResponse = JsonConvert.DeserializeObject<TmdbSeasonTvResponse>(await response.Content.ReadAsStringAsync());

                        if(seasonResponse.Episodes.Count == 0)
                        {
                            continue;
                        }

                        saga.Titles[LocalizationEnum.English] = seasonResponse.Name;
                        saga.Descriptions[LocalizationEnum.English] = seasonResponse.Description;

                        saga.EpisodeFrom = seasonResponse.Episodes.Select(x => x.EpisodeNumber).Min();
                        saga.EpisodeTo = seasonResponse.Episodes.Select(x => x.EpisodeNumber).Max();
                        saga.EpisodesCount = seasonResponse.Episodes.Count;
                    }

                    url = $"/3/tv/{anime.TmdbId}/season/{i}/translations";
                    using (var response = await this._tmdbClient.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();

                        TmdbTranslationsResponse seasonResponse = JsonConvert.DeserializeObject<TmdbTranslationsResponse>(await response.Content.ReadAsStringAsync());

                        foreach(var t in seasonResponse.Translations)
                        {
                            string locale = LocalizationEnum.FormatIsoToLocale(t.Iso639);
                            if (locale != LocalizationEnum.English && LocalizationEnum.IsLocaleSupported(locale))
                            {
                                saga.Titles[locale] = t.Data.Name;
                                saga.Descriptions[locale] = t.Data.Overview;
                            }
                        }
                    }

                    if(i > 1)
                    {
                        Anime.Saga prevSaga = anime.Sagas[i - 2];
                            
                        if (saga.EpisodeFrom < prevSaga.EpisodeTo)
                        {
                            break;
                        }
                    }

                    anime.Sagas.Add(saga);
                }
            }
            catch(Exception ex)
            {
#if DEBUG
                this.Log(ex.Message);
#endif
            }

            return anime;
        }

        async Task<Anime> linkMovieToTMDB(Anime anime)
        {
            try
            {
                List<TmdbSearchMovieResponse.ResponseResult> results = new List<TmdbSearchMovieResponse.ResponseResult>();
                int lastPage = int.MaxValue;

                for (int page = 1; page < lastPage; page++)
                {
                    string url = $"/3/search/movie?query={HttpUtility.UrlEncode(anime.Titles[LocalizationEnum.Romaji])}&page={page}";
                    using (var response = await this._tmdbClient.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();

                        TmdbSearchMovieResponse searchResponse = JsonConvert.DeserializeObject<TmdbSearchMovieResponse>(await response.Content.ReadAsStringAsync());

                        if (lastPage == int.MaxValue)
                        {
                            lastPage = searchResponse.TotalPages;
                        }

                        results.AddRange(searchResponse.Results);
                    }
                }

                TmdbSearchMovieResponse.ResponseResult match = null;
                bool exactMatch = false;

                foreach (var result in results)
                {
                    if (exactMatch == true)
                    {
                        continue;
                    }

                    List<TmdbAlternativeTitlesResponse.ResponseResult> titles = new List<TmdbAlternativeTitlesResponse.ResponseResult>();

                    string url = $"/3/movie/{result.Id}/alternative_titles";
                    using (var response = await this._tmdbClient.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();

                        TmdbAlternativeTitlesResponse titlesResponse = JsonConvert.DeserializeObject<TmdbAlternativeTitlesResponse>(await response.Content.ReadAsStringAsync());
                        titles = titlesResponse.Results;
                    }

                    string title = anime.Titles[LocalizationEnum.English] ?? anime.Titles[LocalizationEnum.Romaji];
                    List<string> matchTitles = new List<string> { result.Name };

                    if(titles != null)
                    {
                        foreach (var t in titles.Where(x => x.Iso3166 == "JP" || x.Iso3166 == "US"))
                        {
                            matchTitles.Add(t.Title);
                        }
                    }

                    foreach (string matchTitle in matchTitles)
                    {
                        if (title.ToLower() == matchTitle.ToLower())
                        {
                            if (anime.StartDate.HasValue &&
                                result.ReleaseDate.HasValue)
                            {
                                if (anime.StartDate.Value.Year == result.ReleaseDate.Value.Year)
                                {
                                    match = result;
                                    exactMatch = true;
                                    break;
                                }
                            }
                            else if (!exactMatch)
                            {
                                match = result;
                            }
                        }
                    }
                }

                if (match != null)
                {
                    anime.TmdbId = match.Id;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                this.Log(ex.Message);
#endif
            }

            return anime;
        }

        async Task<Anime> getMovieTMBDInfo(Anime anime)
        {
            try
            {
                string url = $"/3/movie/{anime.TmdbId}/translations";
                using (var response = await this._tmdbClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();

                    TmdbTranslationsResponse seasonResponse = JsonConvert.DeserializeObject<TmdbTranslationsResponse>(await response.Content.ReadAsStringAsync());

                    foreach (var t in seasonResponse.Translations)
                    {
                        string locale = LocalizationEnum.FormatIsoToLocale(t.Iso639);
                        if (locale != LocalizationEnum.English && LocalizationEnum.IsLocaleSupported(locale))
                        {
                            anime.Titles[locale] = t.Data.Name;
                            anime.Descriptions[locale] = t.Data.Overview;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                this.Log(ex.Message);
#endif
            }

            return anime;
        }
    }
}
