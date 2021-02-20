using Commons.Enums;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Shared.Components
{
    public partial class AnimeCard
    {
        #region Injection

        [Inject] protected UIConfiguration Theme { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }

        #endregion

        [Parameter] public long Id { get; set; }
        [Parameter] public Dictionary<string, string> Titles { get; set; }
        [Parameter] public Dictionary<string, string> Descriptions { get; set; }
        [Parameter] public string CoverColor { get; set; }
        [Parameter] public string CoverImage { get; set; }
        [Parameter] public string BannerImage { get; set; }
        [Parameter] public AnimeUserStatusEnum UserStatus { get; set; }
        [Parameter] public int Score { get; set; }
        [Parameter] public AnimeStatusEnum Status { get; set; }
        [Parameter] public AnimeFormatEnum Format { get; set; }
        [Parameter] public List<string> Genres { get; set; }

        private string Title { get; set; }
        private string Description { get; set; }
        private bool IsTooltipActive { get; set; }

        protected override void OnInitialized()
        {
            Title = Titles.Keys.Contains(Theme.Locale) ? Titles[Theme.Locale] : Titles[Theme.FallbackLocale];
            Description = Descriptions.Keys.Contains(Theme.Locale) ? Descriptions[Theme.Locale] : Descriptions[Theme.FallbackLocale];

            base.OnInitialized();
        }

        private void OnMouseClick()
        {
            NavigationManager.NavigateTo($"/App/Anime/{Id}");
        }

        private void OnShowTooltip()
        {
            IsTooltipActive = true;

            StateHasChanged();
        }

        private void OnHideTooltip()
        {
            IsTooltipActive = false;

            StateHasChanged();
        }
    }
}
