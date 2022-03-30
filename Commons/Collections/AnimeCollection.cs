using Commons.Filters;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using MongoService;
using Commons.Enums;

namespace Commons.Collections
{
    public class AnimeCollection : ICollection<Anime>
    {
        protected override string CollectionName => "anime";

        public override void Add(ref Anime document)
        {
            base.Add(ref document);

            this.Collection.InsertOne(document);
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<Anime>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public override void Edit(ref Anime document)
        {
            base.Edit(ref document);

            var filter = Builders<Anime>.Filter.Eq("_id", document.Id);
            this.Collection.ReplaceOne(filter, document);
        }

        public override Anime Get(long id)
        {
            return this.Collection.Find(x => x.Id == id).FirstOrDefault();
        }

        public override bool Exists(ref Anime document, bool updateValues = true)
        {
            int anilistId = document.AnilistId;
            Anime reference = this.Collection.Find(x => x.AnilistId == anilistId).FirstOrDefault();

            if(reference != null)
            {
                if (updateValues)
                {
                    document.Id = reference.Id;
                    document.CreationDate = reference.CreationDate;
                    document.UpdateDate = reference.UpdateDate;
                    document.HasEpisodes = reference.HasEpisodes;

                    foreach(var locale in document.Titles.Keys)
                    {
                        if (reference.Titles.ContainsKey(locale) && 
                            !string.IsNullOrEmpty(reference.Titles[locale]) &&
                            !document.Titles.ContainsKey(locale))
                        {
                            document.Titles[locale] = reference.Titles[locale];
                        }
                    }

                    foreach (var locale in document.Descriptions.Keys)
                    {
                        if (reference.Descriptions.ContainsKey(locale) && 
                            !string.IsNullOrEmpty(reference.Descriptions[locale]) &&
                            !document.Titles.ContainsKey(locale))
                        {
                            document.Descriptions[locale] = reference.Descriptions[locale];
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public override Paging<Anime> GetList<TFilter>(IFilter<TFilter> filter)
        {
            AnimeFilter animeFilter = filter as AnimeFilter;

            var builder = Builders<Anime>.Filter;
            FilterDefinition<Anime> queryFilter = builder.Empty;

            animeFilter.ApplyBaseFilter(builder, ref queryFilter);

            if (!string.IsNullOrEmpty(animeFilter.title))
            {
                queryFilter &= builder.Regex($"titles.{animeFilter.locale}", new BsonRegularExpression($".*{animeFilter.title}.*", "i"));
            }

            if(animeFilter.anilist_id != null)
            {
                queryFilter &= builder.Eq("anilist_id", animeFilter.anilist_id);
            }

            if(animeFilter.mal_id != null)
            {
                queryFilter &= builder.Eq("mal_id", animeFilter.mal_id);
            }

            if(animeFilter.tmbd_id != null)
            {
                queryFilter &= builder.Eq("tmdb_id", animeFilter.tmbd_id);
            }

            if(animeFilter.formats.Count > 0)
            {
                queryFilter &= builder.AnyIn("format", animeFilter.formats);
            }

            if(animeFilter.status != null)
            {
                queryFilter &= builder.Eq("status", animeFilter.status);
            }

            if(animeFilter.year != null)
            {
                queryFilter &= builder.Eq("season_year", animeFilter.year);
            }

            if(animeFilter.season != null)
            {
                queryFilter &= builder.Eq("season_period", animeFilter.season);
            }

            if(animeFilter.genres.Count > 0)
            {
                queryFilter &= builder.AnyIn("genres", animeFilter.genres);
            }

            if(animeFilter.nsfw == false)
            {
                queryFilter &= builder.Nin("genres", new System.Collections.Generic.List<string> { "Hentai", "Nudity", "Ecchi", "Drugs", "Rape", "Handjob" });
            }

            if(animeFilter.with_episodes == true)
            {
                queryFilter &= builder.Eq("has_episodes", true);
            }

            if (!string.IsNullOrEmpty(animeFilter.locale))
            {
                queryFilter &= builder.Exists($"titles.{animeFilter.locale}");
            }

            SortDefinition<Anime> sort = animeFilter.ApplySort<Anime>(
                new System.Collections.Generic.List<string>
                {
                    "score"
                },
                new System.Collections.Generic.List<short>
                {
                    -1
                });

            return new Paging<Anime>(this.Collection, animeFilter.page, queryFilter, sort, animeFilter.per_page);
        }
    }
}
