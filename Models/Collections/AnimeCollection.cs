using Models.Filters;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoService;
using System;

namespace Models.Collections
{
    public class AnimeCollection : ICollection<Anime>
    {
        protected override string CollectionName => "anime";

        public override void Add(ref Anime document)
        {
            document.Id = this.CalcNewId();
            document.CreationDate = DateTime.Now;
            document.UpdateDate = null;

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
            document.UpdateDate = DateTime.Now;
            
            var filter = Builders<Anime>.Filter.Eq("_id", document.Id);
            this.Collection.ReplaceOne(filter, document);
        }

        public override Anime Get(long id)
        {
            return this.Collection.Find(x => x.Id == id).FirstOrDefault();
        }

        public override bool Exists(ref Anime document)
        {
            int anilistId = document.AnilistId;
            Anime reference = this.Collection.Find(x => x.AnilistId == anilistId).FirstOrDefault();

            if(reference != null)
            {
                document.Id = reference.Id;
                document.CreationDate = reference.CreationDate;
                document.UpdateDate = reference.UpdateDate;
                return true;
            }

            return false;
        }

        public override Paging<Anime> GetList<TFilter>(IFilter<TFilter> filter)
        {
            AnimeFilter animeFilter = filter as AnimeFilter;

            var builder = Builders<Anime>.Filter;
            FilterDefinition<Anime> queryFilter = builder.Empty;

            if (!string.IsNullOrEmpty(animeFilter.Title))
            {
                queryFilter = queryFilter & builder.Regex($"titles.{animeFilter.Locale}", new BsonRegularExpression($".*{animeFilter.Title}.*"));
            }

            if(animeFilter.AnilistId != 0)
            {
                queryFilter = queryFilter & builder.Eq("anilist_id", animeFilter.AnilistId);
            }

            return new Paging<Anime>(this.Collection, animeFilter.Page, queryFilter);
        }
    }
}
