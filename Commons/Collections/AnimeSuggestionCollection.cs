using Commons.Filters;
using MongoDB.Driver;
using MongoService;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Collections
{
    public class AnimeSuggestionCollection : ICollection<AnimeSuggestion>
    {
        protected override string CollectionName => "anime_suggestion";

        public override void Add(ref AnimeSuggestion document)
        {
            base.Add(ref document);

            this.Collection.InsertOne(document);
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<AnimeSuggestion>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public override void Edit(ref AnimeSuggestion document)
        {
            base.Edit(ref document);

            var filter = Builders<AnimeSuggestion>.Filter.Eq("_id", document.Id);
            this.Collection.ReplaceOne(filter, document);
        }

        public override bool Exists(ref AnimeSuggestion document, bool updateValues = true)
        {
            long animeId = document.AnimeID;
            string source = document.Source;
            string title = document.Title;

            AnimeSuggestion reference = this.Collection.Find(x => x.AnimeID == animeId && x.Source == source && x.Title == title).FirstOrDefault();

            if(reference != null)
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

        public override AnimeSuggestion Get(long id)
        {
            throw new NotImplementedException();
        }

        public override Paging<AnimeSuggestion> GetList<TFilter>(IFilter<TFilter> filter)
        {
            AnimeSuggestionFilter animeSuggestionFilter = filter as AnimeSuggestionFilter;

            var builder = Builders<AnimeSuggestion>.Filter;
            FilterDefinition<AnimeSuggestion> queryFilter = builder.Empty;

            animeSuggestionFilter.ApplyBaseFilter(builder, ref queryFilter);

            if (animeSuggestionFilter.anime_id != null)
            {
                queryFilter &= builder.Eq("anime_id", animeSuggestionFilter.anime_id);
            }

            if (!string.IsNullOrEmpty(animeSuggestionFilter.title))
            {
                queryFilter &= builder.Eq("title", animeSuggestionFilter.title);
            }

            if (!string.IsNullOrEmpty(animeSuggestionFilter.source))
            {
                queryFilter &= builder.Eq("source", animeSuggestionFilter.source);
            }

            SortDefinition<AnimeSuggestion> sort = animeSuggestionFilter.ApplySort<AnimeSuggestion>(
                new System.Collections.Generic.List<string>
                {
                    "_id"
                },
                new System.Collections.Generic.List<short>
                {
                    1
                });

            return new Paging<AnimeSuggestion>(this.Collection, animeSuggestionFilter.page, queryFilter, sort, animeSuggestionFilter.per_page);
        }
    }
}
