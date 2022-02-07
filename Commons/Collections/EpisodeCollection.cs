using Commons.Filters;
using MongoDB.Driver;
using MongoService;
using ServiceMongo;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Collections
{
    public class EpisodeCollection : ICollection<Episode>
    {
        protected override string CollectionName => "episode";

        public override void Add(ref Episode document)
        {
            try
            {
                base.Add(ref document);

                this.Collection.InsertOne(document);
            }
            catch (MongoException)
            {
                throw new ConcurrencyException();
            }
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<Episode>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public override void Edit(ref Episode document)
        {
            base.Edit(ref document);

            var filter = Builders<Episode>.Filter.Eq("_id", document.Id);
            this.Collection.ReplaceOne(filter, document);
        }

        public override bool Exists(ref Episode document, bool updateValues = true)
        {
            long animeId = document.AnimeID;
            int number = document.Number;
            bool isDub = document.IsDub;
            string locale = document.Locale;
            string quality = document.Quality;

            Episode reference = this.Collection.Find(x => x.AnimeID == animeId &&
                x.Number == number &&
                x.Locale == locale &&
                x.IsDub == isDub &&
                x.Quality == quality).FirstOrDefault();

            if (reference != null)
            {
                if (updateValues)
                {
                    document.Id = reference.Id;
                    document.CreationDate = reference.CreationDate;
                    document.UpdateDate = reference.UpdateDate;
                }
                return true;
            }

            return false;
        }

        public override Episode Get(long id)
        {
            return this.Collection.Find(x => x.Id == id).FirstOrDefault();
        }

        public override Paging<Episode> GetList<TFilter>(IFilter<TFilter> filter)
        {
            EpisodeFilter episodeFilter = filter as EpisodeFilter;

            var builder = Builders<Episode>.Filter;
            FilterDefinition<Episode> queryFilter = builder.Empty;

            episodeFilter.ApplyBaseFilter(builder, ref queryFilter);

            if (episodeFilter.anime_id != null)
            {
                queryFilter &= builder.Eq("anime_id", episodeFilter.anime_id);
            }

            if(episodeFilter.number != null)
            {
                queryFilter &= builder.Eq("number", episodeFilter.number);
            }

            if(episodeFilter.is_dub != null)
            {
                queryFilter &= builder.Eq("is_dub", episodeFilter.is_dub);
            }

            if (!string.IsNullOrEmpty(episodeFilter.locale))
            {
                queryFilter &= builder.Eq("locale", episodeFilter.locale);  
            }

            SortDefinition<Episode> sort = episodeFilter.ApplySort<Episode>(
                new System.Collections.Generic.List<string>
                {
                    "anime_id",
                    "number"
                },
                new System.Collections.Generic.List<short>
                {
                    1,
                    1
                });

            return new Paging<Episode>(this.Collection, episodeFilter.page, queryFilter, sort, episodeFilter.per_page);
        }
    }
}
