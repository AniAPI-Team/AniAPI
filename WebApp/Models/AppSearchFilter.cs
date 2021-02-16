using Blazored.LocalStorage;
using Commons.Enums;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class AppSearchFilter
    {
        public Dictionary<string, string> AvailableGenres { get; set; }
        public Dictionary<string, string> AvailableYears { get; set; }
        public Dictionary<string, string> AvailableSeasons { get; set; }
        public Dictionary<string, string> AvailableFormats { get; set; }
        public Dictionary<string, string> AvailableStatus { get; set; }

        public FilterValues Values { get; set; }

        public AppSearchFilter()
        {
            AvailableGenres = new Dictionary<string, string>()
            {
                { "Action", "Action" },
                { "Adventure", "Adventure" },
                { "Comedy", "Comedy" },
                { "Drama", "Drama" },
                { "Ecchi", "Ecchi" },
                { "Fantasy", "Fantasy" },
                { "Horror", "Horror" },
                { "Mahou Shoujo", "Mahou Shoujo" },
                { "Mecha", "Mecha" },
                { "Music", "Music" },
                { "Mystery", "Mystery" },
                { "Psychological", "Psychological" },
                { "Romance", "Romance" },
                { "Sci-Fi", "Sci-Fi" },
                { "Slice Of Life", "Slice Of Life" },
                { "Sports", "Sports" },
                { "Supernatural", "Supernatural" },
                { "Thriller", "Thriller" }
            };

            AvailableYears = new Dictionary<string, string>();
            for (int i = DateTime.Now.Year + 1; i >= 1940; i--)
            {
                AvailableYears.Add(i.ToString(), i.ToString());
            }

            AvailableSeasons = new Dictionary<string, string>()
            {
                { AnimeSeasonEnum.WINTER.ToString(), "Winter" },
                { AnimeSeasonEnum.SPRING.ToString(), "Spring" },
                { AnimeSeasonEnum.SUMMER.ToString(), "Summer" },
                { AnimeSeasonEnum.FALL.ToString(), "Fall" },
            };

            AvailableFormats = new Dictionary<string, string>()
            {
                { AnimeFormatEnum.TV.ToString(), "TV Show" },
                { AnimeFormatEnum.MOVIE.ToString(), "Movie" },
                { AnimeFormatEnum.TV_SHORT.ToString(), "TV Short" },
                { AnimeFormatEnum.SPECIAL.ToString(), "Special" },
                { AnimeFormatEnum.OVA.ToString(), "OVA" },
                { AnimeFormatEnum.ONA.ToString(), "ONA" }
            };

            AvailableStatus = new Dictionary<string, string>()
            {
                { AnimeStatusEnum.RELEASING.ToString(), "Airing" },
                { AnimeStatusEnum.FINISHED.ToString(), "Ended" },
                { AnimeStatusEnum.NOT_YET_RELEASED.ToString(), "TBA" },
                { AnimeStatusEnum.CANCELLED.ToString(), "Cancelled" },
            };

            Values = new FilterValues();
        }

        public void ApplyQueryString(NavigationManager navigationManager)
        {
            if (navigationManager.TryGetQueryString<string>("title", out string title))
            {
                Values.Title = title;
            }

            if (navigationManager.TryGetQueryString<string>("genres", out string genres))
            {
                foreach (string genre in genres.Split(','))
                {
                    Values.Genres.Add(genre);
                }
            }

            if (navigationManager.TryGetQueryString<string>("year", out string year))
            {
                Values.Year = year;
            }

            if (navigationManager.TryGetQueryString<string>("season", out string season))
            {
                Values.Season = season;
            }

            if (navigationManager.TryGetQueryString<string>("format", out string format))
            {
                Values.Format = format;
            }

            if (navigationManager.TryGetQueryString<string>("status", out string status))
            {
                Values.Status = status;
            }
        }

        public void GetValuesFromLocalStorage(ISyncLocalStorageService localStorage)
        {
            if (localStorage.ContainKey("app_search_filter"))
            {
                Values = localStorage.GetItem<FilterValues>("app_search_filter");
            }
            else
            {
                Values = new FilterValues();
            }
        }

        public void SaveValuesInLocalStorage(ISyncLocalStorageService localStorage)
        {
            if(Values == null)
            {
                localStorage.RemoveItem("app_search_filter");
            }
            else
            {
                localStorage.SetItem<FilterValues>("app_search_filter", Values);
            }
        }

        public List<string> GetTagList()
        {
            List<string> tagList = new List<string>();

            if (!string.IsNullOrEmpty(Values.Title))
            {
                tagList.Add(Values.Title);
            }

            tagList.AddRange(Values.Genres);

            if (!string.IsNullOrEmpty(Values.Year))
            {
                tagList.Add(Values.Year);
            }

            if (!string.IsNullOrEmpty(Values.Season))
            {
                tagList.Add(AvailableSeasons[Values.Season]);
            }

            if (!string.IsNullOrEmpty(Values.Format))
            {
                tagList.Add(AvailableFormats[Values.Format]);
            }

            if (!string.IsNullOrEmpty(Values.Status))
            {
                tagList.Add(AvailableStatus[Values.Status]);
            }

            return tagList;
        }

        public class FilterValues
        {
            public string Title { get; set; }
            public List<string> Genres { get; set; } = new List<string>();
            public string Year { get; set; }
            public string Season { get; set; }
            public string Format { get; set; }
            public string Status { get; set; }
            public bool HideWatched { get; set; } = true;
        }
    }
}
