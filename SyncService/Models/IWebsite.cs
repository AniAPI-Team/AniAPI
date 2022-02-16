using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using FuzzySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SyncService.Models
{
    public abstract class IWebsite : Website
    {
        public IWebsite(Website website)
        {
            Id = website.Id;
            CreationDate = website.CreationDate;
            UpdateDate = website.UpdateDate;
            Name = website.Name;
            Active = website.Active;
            Official = website.Official;
            SiteUrl = website.SiteUrl;
            Localization = website.Localization;
        }

        private AnimeSuggestionCollection _animeSuggestionCollection = new AnimeSuggestionCollection();

        public virtual bool AnalyzeMatching(Anime anime, AnimeMatching matching, string sourceTitle)
        {
            matching.Score = Fuzz.TokenSortRatio(matching.Title.ToLower(), sourceTitle.ToLower());

            if (matching.Score == 100)
            {
                return true;
            }
            else if (matching.Score >= 60)
            {
                var query = this._animeSuggestionCollection.GetList(new AnimeSuggestionFilter()
                {
                    anime_id = anime.Id,
                    title = matching.Title,
                    source = this.Name
                });

                if (query.Count > 0)
                {
                    if (query.Documents[0].Status == AnimeSuggestionStatusEnum.OK)
                    {
                        return true;
                    }
                    else if (query.Documents[0].Status == AnimeSuggestionStatusEnum.KO)
                    {
                        return false;
                    }
                }
                else
                {
                    AnimeSuggestion suggestion = new AnimeSuggestion()
                    {
                        AnimeID = anime.Id,
                        Title = matching.Title,
                        Source = this.Name,
                        Score = matching.Score,
                        Path = $"{(this.SiteUrl.Last() == '/' ? this.SiteUrl.Substring(0, this.SiteUrl.Length - 1) : this.SiteUrl)}{matching.Path}",
                        Status = AnimeSuggestionStatusEnum.NONE
                    };

                    this._animeSuggestionCollection.Add(ref suggestion);
                }
            }

            return false;
        }

        public abstract Dictionary<string, string> GetVideoProxyHeaders(AnimeMatching matching, Dictionary<string, string> values = null);

        //public virtual string BuildAPIProxyURL(AppSettings settings, AnimeMatching matching, string url, Dictionary<string, string> values = null)
        //{
        //    string apiUrl = $"{settings.APIEndpoint}/proxy/{HttpUtility.UrlEncode(url)}/";

        //    if (values != null)
        //    {
        //        apiUrl += "?";
        //    }

        //    for (int i = 0; i < values?.Keys.Count; i++)
        //    {
        //        string key = values.Keys.ElementAt(i);

        //        apiUrl += $"{key}={HttpUtility.UrlEncode(values[key])}";

        //        if ((i + 1) < values.Keys.Count)
        //        {
        //            apiUrl += "&";
        //        }
        //    }

        //    return apiUrl;
        //}
    }
}
