using Commons;
using Commons.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncService.Models.Trackers
{
    public class AnilistTracker : ITracker
    {
        public AnilistTracker(HttpClient client) : base(client)
        {
        }

        public override string Name => "anilist";

        public override async Task Export(UserStory s)
        {
            GraphQLQuery query = new GraphQLQuery()
            {
                Query = @"
                mutation($mediaId: Int, $status: MediaListStatus, $progress: Int) {
                    SaveMediaListEntry(mediaId: $mediaId, status: $status, progress: $progress) {
                        id
                    }
                }
                ",
                Variables = new Dictionary<string, object>()
                {
                    { "mediaId", Anime.AnilistId },
                    { "status", s.Status.ToString() },
                    { "progress", s.CurrentEpisode }
                }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.AnilistToken);

            using (var response = await client.SendAsync(request))
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Thread.Sleep(60 * 1000);
                    }
                    
                    return;
                }

                HasDone = true;
            }
        }

        public override async Task<string> GetAvatar()
        {
            GraphQLQuery query = new GraphQLQuery()
            {
                Query = @"
                query($userId: Int) {
                    User(id: $userId) {
                        avatar {
                            large
                        }
                    }
                }
                ",
                Variables = new Dictionary<string, object>()
                {
                    { "userId", User.AnilistId }
                }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.AnilistToken);

            using (var response = await client.SendAsync(request))
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Thread.Sleep(60 * 1000);
                    }

                    return null;
                }

                AnilistAvatarResponse anilistResponse = JsonConvert.DeserializeObject<AnilistAvatarResponse>(await response.Content.ReadAsStringAsync());

                HasDone = true;

                return anilistResponse.Data.User.Avatar.Large;
            }
        }

        public override async Task<List<UserStory>> Import()
        {
            List<UserStory> imported = new List<UserStory>();

            GraphQLQuery query = new GraphQLQuery()
            {
                Query = @"
                query($userId: Int) {
                    MediaListCollection(userId: $userId, type: ANIME) {
                        lists {
                            isCustomList
                            entries {
                                mediaId
                                status
                                progress
                            }
                        }
                    }
                }
                ",
                Variables = new Dictionary<string, object>()
                {
                    { "userId", User.AnilistId }
                }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", User.AnilistToken);

            using (var response = await client.SendAsync(request))
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Thread.Sleep(60 * 1000);
                    }

                    return new List<UserStory>();
                }

                AnilistImportResponse anilistResponse = JsonConvert.DeserializeObject<AnilistImportResponse>(await response.Content.ReadAsStringAsync());

                foreach (AnilistImportResponse.ResponseMediaList list in anilistResponse.Data.MediaListCollection.Lists)
                {
                    if (list.IsCustomList)
                    {
                        continue;
                    }

                    foreach (AnilistImportResponse.ResponseMedia media in list.Entries)
                    {
                        var intQuery = this._animeCollection.GetList(new AnimeFilter()
                        {
                            anilist_id = media.MediaId,
                            page = 1
                        });

                        if (intQuery.Count > 0)
                        {
                            long animeId = intQuery.Documents[0].Id;

                            imported.Add(new UserStory()
                            {
                                UserID = User.Id,
                                AnimeID = animeId,
                                Status = media.Status,
                                CurrentEpisode = media.Progress,
                                CurrentEpisodeTicks = 0,
                                Synced = true
                            });
                        }
                    }
                }

                HasDone = true;
            }

            return imported;
        }

        public override bool NeedWork()
        {
            return User.HasAnilist.Value;
        }
    }
}
