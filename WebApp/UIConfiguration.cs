using Blazored.LocalStorage;
using Commons.Enums;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp
{
    public class UIConfiguration
    {
        #region Injection

        protected ISyncLocalStorageService LocalStorage { get; set; }

        #endregion

        public UIConfiguration(bool useCustomVideoPlayer, bool useDarkTheme)
        {
            UseCustomVideoPlayer = useCustomVideoPlayer;
            UseDarkTheme = useDarkTheme;
            Locale = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

        public bool UseCustomVideoPlayer { get; set; }
        public bool UseDarkTheme { get; set; }
        public string Locale { get; set; }
        public string FallbackLocale => "en";

        public string GetHexByAnimeUserStatus(AnimeUserStatusEnum userStatus)
        {
            switch (userStatus)
            {
                case AnimeUserStatusEnum.COMPLETED:
                    return "#7bd555";
                case AnimeUserStatusEnum.CURRENT:
                    return "#3db4f2";
                case AnimeUserStatusEnum.PAUSED:
                    return "#f7bf63";
                case AnimeUserStatusEnum.DROPPED:
                    return "#e85d75";
                case AnimeUserStatusEnum.PLANNING:
                    return "#f79a63";
            }

            return string.Empty;
        }

        public string GetTextByAnimeUserStatus(AnimeUserStatusEnum userStatus)
        {
            switch (userStatus)
            {
                case AnimeUserStatusEnum.COMPLETED:
                    return "Watched";
                case AnimeUserStatusEnum.CURRENT:
                    return "Watching";
                case AnimeUserStatusEnum.PAUSED:
                    return "Paused";
                case AnimeUserStatusEnum.DROPPED:
                    return "Stopped";
                case AnimeUserStatusEnum.PLANNING:
                    return "Planned";
            }

            return string.Empty;
        }

        public string GetTextByAnimeStatus(AnimeStatusEnum animeStatus)
        {
            switch (animeStatus)
            {
                case AnimeStatusEnum.FINISHED:
                    return "Ended";
                case AnimeStatusEnum.RELEASING:
                    return "Airing";
                case AnimeStatusEnum.NOT_YET_RELEASED:
                    return "TBA";
                case AnimeStatusEnum.CANCELLED:
                    return "Cancelled";
            }

            return string.Empty;
        }

        public string GetTextByAnimeFormat(AnimeFormatEnum animeFormat)
        {
            switch (animeFormat)
            {
                case AnimeFormatEnum.TV:
                    return "TV Show";
                case AnimeFormatEnum.MOVIE:
                    return "Movie";
                case AnimeFormatEnum.TV_SHORT:
                    return "TV Short";
                case AnimeFormatEnum.SPECIAL:
                    return "Special";
                case AnimeFormatEnum.OVA:
                    return "OVA";
                case AnimeFormatEnum.ONA:
                    return "ONA";
            }

            return string.Empty;
        }

        public string GetTextByAnimeSeason(AnimeSeasonEnum animeSeason)
        {
            switch (animeSeason)
            {
                case AnimeSeasonEnum.WINTER:
                    return "Winter";
                case AnimeSeasonEnum.SPRING:
                    return "Spring";
                case AnimeSeasonEnum.SUMMER:
                    return "Summer";
                case AnimeSeasonEnum.FALL:
                    return "Fall";
            }

            return string.Empty;
        }
    }
}
