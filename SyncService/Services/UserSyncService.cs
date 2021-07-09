using Commons;
using Commons.Collections;
using Commons.Filters;
using Newtonsoft.Json;
using SyncService.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncService.Services
{
    public class UserSyncService : IService
    {
        #region Members

        private UserCollection _userCollection = new UserCollection();
        private UserStoryCollection _userStoryCollection = new UserStoryCollection();
        private AnimeCollection _animeCollection = new AnimeCollection();
        private User _user;
        private HttpClient _anilistClient = new HttpClient() { BaseAddress = new Uri("https://graphql.anilist.co") };

        protected override int TimeToWait => 60 * 1000; // 1 Minute

        #endregion

        protected override ServicesStatus GetServiceStatus()
        {
            return new ServicesStatus("UserSync");
        }

        public override async Task Work()
        {
            await base.Work();

            try
            {
                long lastID = this._userCollection.Last() != null ? this._userCollection.Last().Id : 0;

                for(long userID = 1; userID <= lastID; userID++)
                {
                    try
                    {
                        _user = this._userCollection.Get(userID);

                        if (_user == null)
                        {
                            continue;
                        }

                        _user.CalcDerivedFields();

                        if (!_user.HasAnilist.Value && !_user.HasMyAnimeList.Value)
                        {
                            throw new Exception();
                        }

                        List<UserStory> toImport = new List<UserStory>();

                        if (_user.HasAnilist.Value)
                        {
                            toImport.AddRange(await this.importFromAnilist());
                        }

                        if (_user.HasMyAnimeList.Value)
                        {
                            toImport.AddRange(await this.importFromMyAnimeList());
                        }

                        for (int i = 0; i < toImport.Count; i++)
                        {
                            UserStory s = toImport[i];

                            if (this._userStoryCollection.Exists(ref s))
                            {
                                if (s.Synced)
                                {
                                    this._userStoryCollection.Edit(ref s);
                                }
                            }
                            else
                            {
                                this._userStoryCollection.Add(ref s);
                            }
                        }

                        int lastPage = 1;
                        for (int page = 1; page <= lastPage; page++)
                        {
                            var query = this._userStoryCollection.GetList(new UserStoryFilter()
                            {
                                user_id = userID,
                                synced = false
                            });

                            lastPage = query.LastPage;

                            if (query.Count == 0)
                            {
                                continue;
                            }

                            for (int i = 0; i < query.Documents.Count; i++)
                            {
                                UserStory s = query.Documents[i];

                                if (_user.HasAnilist.Value)
                                {
                                    await this.exportToAnilist(s);
                                }

                                if (_user.HasMyAnimeList.Value)
                                {
                                    await this.exportToMyAnimeList(s);
                                }

                                s.Synced = true;
                                this._userStoryCollection.Edit(ref s);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        this.Error(ex.Message);
                    }
                    finally
                    {
                        this.Log($"Done {this.GetProgressD(userID, lastID)}%", true);
                    }

                    if (_cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException("Process cancellation requested!");
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        private async Task<List<UserStory>> importFromAnilist()
        {
            List<UserStory> imported = new List<UserStory>();
            bool done = false;

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
                    { "userId", _user.AnilistId }
                }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _user.AnilistToken);

            do
            {
                using (var response = await _anilistClient.SendAsync(request))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception ex)
                    {
                        if(response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            Thread.Sleep(60 * 1000);
                            continue;
                        }
                            
                        throw;
                    }

                    done = true;

                    AnilistImportResponse anilistResponse = JsonConvert.DeserializeObject<AnilistImportResponse>(await response.Content.ReadAsStringAsync());

                    foreach(AnilistImportResponse.ResponseMediaList list in anilistResponse.Data.MediaListCollection.Lists)
                    {
                        if (list.IsCustomList)
                        {
                            continue;
                        }

                        foreach(AnilistImportResponse.ResponseMedia media in list.Entries)
                        {
                            var intQuery = this._animeCollection.GetList(new AnimeFilter()
                            {
                                anilist_id = media.MediaId,
                                page = 1
                            });

                            if(intQuery.Count > 0)
                            {
                                long animeId = intQuery.Documents[0].Id;
                                
                                imported.Add(new UserStory()
                                {
                                    UserID = _user.Id,
                                    AnimeID = animeId,
                                    Status = media.Status,
                                    CurrentEpisode = media.Progress,
                                    CurrentEpisodeTime = TimeSpan.Zero,
                                    Synced = true
                                });
                            }
                        }
                    }
                }
            }
            while (done == false);

            return imported;
        }

        private async Task<List<UserStory>> importFromMyAnimeList()
        {
            throw new NotImplementedException();
        }

        private async Task exportToAnilist(UserStory s)
        {
            bool done = false;

            Anime anime = this._animeCollection.Get(s.AnimeID);

            GraphQLQuery query = new GraphQLQuery()
            {
                Query = @"
                mutation($mediaId: Int, $status: MediaListStatus) {
                    SaveMediaListEntry(mediaId: $mediaId, status: $status) {
                        id
                    }
                }
                ",
                Variables = new Dictionary<string, object>()
                {
                    { "mediaId", anime.AnilistId },
                    { "status", s.Status.ToString() }
                }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _user.AnilistToken);

            do
            {
                using (var response = await _anilistClient.SendAsync(request))
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
                            continue;
                        }

                        throw;
                    }

                    done = true;
                }
            }
            while (done == false);
        }

        private async Task exportToMyAnimeList(UserStory s)
        {
            throw new NotImplementedException();
        }
    }
}
