using Commons;
using Commons.Collections;
using Commons.Filters;
using Newtonsoft.Json;
using SyncService.Models;
using SyncService.Models.Trackers;
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

        private List<ITracker> _trackers;

        protected override int TimeToWait => 60 * 1000; // 1 Minute

        #endregion

        protected override ServicesStatus GetServiceStatus()
        {
            return new ServicesStatus("UserSync");
        }

        public override Task Start(CancellationToken cancellationToken)
        {
            _trackers = new List<ITracker>()
            {
                new AnilistTracker(new HttpClient() { BaseAddress = new Uri("https://graphql.anilist.co") }),
                new MyAnimeListTracker(new HttpClient() { BaseAddress = new Uri("https://api.myanimelist.net/v2/") })
            };

            return base.Start(cancellationToken);
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

                        foreach(ITracker tracker in _trackers)
                        {
                            tracker.User = _user;

                            if (tracker.NeedWork())
                            {
                                do
                                {
                                    toImport.AddRange(await tracker.Import());
                                }
                                while (tracker.HasDone == false);
                            }
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

                                foreach (ITracker tracker in _trackers)
                                {
                                    tracker.User = _user;
                                    tracker.Anime = this._animeCollection.Get(s.AnimeID);

                                    if (tracker.NeedWork())
                                    {
                                        do
                                        {
                                            await tracker.Export(s);
                                        }
                                        while (tracker.HasDone == false);
                                    }
                                }

                                s.Synced = true;
                                this._userStoryCollection.Edit(ref s);
                            }
                        }

                        foreach(ITracker tracker in _trackers)
                        {
                            if (tracker.Name == _user.AvatarTracker)
                            {
                                do
                                {
                                    _user.Avatar = await tracker.GetAvatar();
                                }
                                while (tracker.HasDone == false);
                                
                                this._userCollection.Edit(ref _user);
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
    }
}
