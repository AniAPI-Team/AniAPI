using Commons;
using Commons.Collections;
using Commons.Enums;
using Commons.Filters;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SyncService.Models.Trackers
{
    public class MyAnimeListTracker : ITracker
    {
        string access_token;

        public MyAnimeListTracker(HttpClient client) : base(client)
        {

        }

        public override string Name => "mal";

        public override async Task Export(UserStory s)
        {
            if (!Anime.MyAnimeListId.HasValue)
            {
                HasDone = true;
                return;
            }

            await getAccessToken();

            if (string.IsNullOrEmpty(access_token))
            {
                HasDone = true;
                return;
            }

            Dictionary<string, string> payload = new Dictionary<string, string>()
            {
                { "status", internalToMalStatus(s.Status) },
                { "num_watched_episodes", s.CurrentEpisode.ToString() }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"anime/{Anime.MyAnimeListId}/my_list_status", UriKind.Relative),
                Content = new FormUrlEncodedContent(payload)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);

            using (var response = await client.SendAsync(request))
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    return;
                }

                HasDone = true;
            }
        }

        public override async Task<string> GetAvatar()
        {
            return string.Empty;
        }

        public override async Task<List<UserStory>> Import()
        {
            List<UserStory> imported = new List<UserStory>();

            await getAccessToken();

            if (string.IsNullOrEmpty(access_token))
            {
                HasDone = true;
                return new List<UserStory>();
            }

            string uri = "users/@me/animelist?fields=list_status&limit=1000";

            do
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri, UriKind.Relative)
                };
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);

                using (var response = await client.SendAsync(request))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception ex)
                    {
                        return new List<UserStory>();
                    }

                    MyAnimeListImportResponse malResponse = JsonConvert.DeserializeObject<MyAnimeListImportResponse>(await response.Content.ReadAsStringAsync());

                    foreach (MyAnimeListImportResponse.ResponseData data in malResponse.Data)
                    {
                        var intQuery = this._animeCollection.GetList(new AnimeFilter()
                        {
                            mal_id = data.Node.AnimeID,
                            page = 1
                        });

                        if (intQuery.Count > 0)
                        {
                            long animeId = intQuery.Documents[0].Id;

                            imported.Add(new UserStory()
                            {
                                UserID = User.Id,
                                AnimeID = animeId,
                                Status = data.InternalStatus,
                                CurrentEpisode = data.Status.EpisodesWatched,
                                CurrentEpisodeTicks = 0,
                                Synced = true
                            });
                        }
                    }

                    if (!string.IsNullOrEmpty(malResponse.Paging.Next))
                    {
                        string[] parts = malResponse.Paging.Next.Split('/');
                        string original = uri.Split('/').Last();

                        uri = uri.Replace(original, parts.Last());
                    }
                    else
                    {
                        uri = null;
                    }
                }
            }
            while (uri != null);
            
            HasDone = true;
            
            return imported;
        }

        public override bool NeedWork()
        {
            return User.HasMyAnimeList.Value;
        }

        async Task getAccessToken()
        {
            access_token = null;

            AppSettings settings = new AppSettingsCollection().Get(0);

            Dictionary<string, string> payload = new Dictionary<string, string>()
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", User.MyAnimeListToken }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(payload)
            };
            string credentials = string.Format("{0}:{1}", settings.MyAnimeList.ClientID, settings.MyAnimeList.ClientSecret);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));

            HttpClient authClient = new HttpClient() { BaseAddress = new Uri("https://myanimelist.net/v1/oauth2/token") };
            try
            {
                using (var response = await authClient.SendAsync(request))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception ex)
                    {
                        return;
                    }

                    Dictionary<string, string> malResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
                    access_token = malResponse["access_token"];
                    User.MyAnimeListToken = malResponse["refresh_token"];
                }
            }
            catch(Exception ex)
            {
                return;
            }
        }

        string internalToMalStatus(UserStoryStatusEnum status)
        {
            switch (status)
            {
                case UserStoryStatusEnum.CURRENT: return "watching";
                case UserStoryStatusEnum.PLANNING: return "plan_to_watch";
                case UserStoryStatusEnum.COMPLETED: return "completed";
                case UserStoryStatusEnum.DROPPED: return "dropped";
                case UserStoryStatusEnum.PAUSED: return "on_hold";
            }

            return string.Empty;
        }
    }
}
