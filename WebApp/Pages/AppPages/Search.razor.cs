using Blazored.LocalStorage;
using Commons;
using Commons.Enums;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Pages.AppPages
{
    public partial class Search : ComponentBase
    {
        #region Injection

        [Inject] protected Generic Generic { get; set; }
        [Inject] protected UIConfiguration Theme { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected ISyncLocalStorageService LocalStorage { get; set; }
        [Inject] protected SpinnerService Spinner { get; set; }

        #endregion

        #region Properties

        private AppSearchFilter Filter;
        private List<string> Tags = new List<string>();
        private List<Anime> AnimeCards = new List<Anime>();

        #endregion

        protected override void OnInitialized()
        {
            Filter = new AppSearchFilter();
            
            Filter.GetValuesFromLocalStorage(LocalStorage);
            Filter.ApplyQueryString(NavigationManager);
            Filter.SaveValuesInLocalStorage(LocalStorage);

            OnParamChanged();
            base.OnInitialized();
        }

        private void OnTitleChanged(string title)
        {
            Filter.Values.Title = title;
            OnParamChanged();
        }

        private void OnGenresChange(List<string> genres)
        {
            Filter.Values.Genres = genres;
            OnParamChanged();
        }

        private void OnYearChange(string year)
        {
            Filter.Values.Year = year;
            OnParamChanged();
        }

        private void OnSeasonChange(string season)
        {
            Filter.Values.Season = season;
            OnParamChanged();
        }

        private void OnFormatChange(string format)
        {
            Filter.Values.Format = format;
            OnParamChanged();
        }

        private void OnStatusChange(string status)
        {
            Filter.Values.Status = status;
            OnParamChanged();
        }

        private void OnHideWatchedChange(bool hideWatched)
        {
            Filter.Values.HideWatched = hideWatched;
            OnParamChanged();
        }

        private void OnParamChanged()
        {
            Filter.SaveValuesInLocalStorage(LocalStorage);
            Tags = Filter.GetTagList();

            AnimeCards = new List<Anime>()
            {
                new Anime()
                {
                    Id = 1,
                    Titles = new Dictionary<string, string>()
                    {
                        { "en", "Naruto: Shippuuden" }
                    },
                    Descriptions = new Dictionary<string, string>()
                    {
                        { "en", "Naruto: Shippuuden is the continuation of the original animated TV series Naruto. The story revolves around an older and slightly more matured Uzumaki Naruto and his quest to save his friend Uchiha Sasuke from the grips of the snake-like Shinobi, Orochimaru. After 2 and a half years Naruto finally returns to his village of Konoha, and sets about putting his ambitions to work, though it will not be easy, as he has amassed a few(more dangerous) enemies, in the likes of the shinobi organization; Akatsuki. <br><br> (Source: Anime News Network)" }
                    },
                    CoverColor = "#e4865d",
                    CoverImage = "https://s4.anilist.co/file/anilistcdn/media/anime/cover/medium/bx1735-80JNLAlnxrKj.png",
                    BannerImage = "https://s4.anilist.co/file/anilistcdn/media/anime/banner/1735.jpg",
                    UserStatus = AnimeUserStatusEnum.COMPLETED,
                    Score = 81,
                    Status = AnimeStatusEnum.FINISHED,
                    Format = AnimeFormatEnum.TV,
                    Genres = new List<string>()
                    {
                        "Action", "Comedy"
                    }
                },
                new Anime()
                {
                    Id = 2,
                    Titles = new Dictionary<string, string>()
                    {
                        { "en", "One Piece" }
                    },
                    Descriptions = new Dictionary<string, string>()
                    {
                        { "en", "Bla bla..." }
                    },
                    CoverColor = "#e4a15d",
                    CoverImage = "https://s4.anilist.co/file/anilistcdn/media/anime/cover/medium/nx21-tXMN3Y20PIL9.jpg",
                    BannerImage = "https://s4.anilist.co/file/anilistcdn/media/anime/banner/21-wf37VakJmZqs.jpg",
                    UserStatus = AnimeUserStatusEnum.PAUSED,
                    Score = 85,
                    Status = AnimeStatusEnum.RELEASING,
                    Format = AnimeFormatEnum.TV,
                    Genres = new List<string>()
                    {
                        "Action", "Adventure", "Comedy"
                    }
                }
            };

            StateHasChanged();
        }
    }
}
