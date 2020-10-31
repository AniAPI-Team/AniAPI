using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Models;
using MongoDB.Bson.IO;
using MongoService;
using MongoService.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace AnimeScraper.Helpers
{
    public class ScraperEngine
    {
        #region Singleton

        private static ScraperEngine _instance;
        public static ScraperEngine Instance
        {
            get
            {
                return _instance ?? (_instance = new ScraperEngine());
            }
        }

        #endregion

        private AnimeCollection _animeCollection = new AnimeCollection();
        private GraphQLHttpClient _anilistClient = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());
        private GraphQLRequest _anilistRequest = new GraphQLRequest()
        {
            Query = @"
            query($page: Int) {
                Page(page: $page, perPage: 50) {
                    pageInfo {
                        lastPage
                    }
                    media {
                        id
                        idMal
                        format
                        status
                        startDate {
                            year
                            month
                            day
                        }
                        endDate {
                            year
                            month
                            day
                        }
                        title {
                            romaji
                            native
                        }
                        description
                    }
                }
            }
            "
        };
        private int _totalPages = 2;

        public async void Start()
        {
            Stopwatch elapsedTime = new Stopwatch();
            elapsedTime.Start();

            for(int currentPage = 1; currentPage <= this._totalPages; currentPage++)
            {
                this._anilistRequest.Variables = new { page = currentPage };

                var response = await this._anilistClient.SendQueryAsync<AnilistResponse>(this._anilistRequest);
                
                if(currentPage == 1)
                {
                    this._totalPages = response.Data.Page.pageInfo.lastPage;
                }

                foreach(AnilistResponse.Media m in response.Data.Page.media)
                {
                    Anime a = new Anime(m);
                    this._animeCollection.Add(ref a);
                }

                Console.WriteLine(currentPage);
                if(currentPage != 1 && (currentPage - 1) % 90 == 0)
                {
                    elapsedTime.Stop();
                    long timeToWait = (60 * 1000) - elapsedTime.ElapsedMilliseconds;

                    if(timeToWait > 0)
                    {
                        Console.WriteLine($"Waiting {timeToWait} ms!");
                        Thread.Sleep((int)timeToWait);
                    }

                    elapsedTime.Start();
                }
            }
        }
    }
}
