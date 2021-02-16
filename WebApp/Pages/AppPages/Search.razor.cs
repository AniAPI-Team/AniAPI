using Blazored.LocalStorage;
using Commons;
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

            StateHasChanged();
        }
    }
}
