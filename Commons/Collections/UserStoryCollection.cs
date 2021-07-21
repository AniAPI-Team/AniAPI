using Commons.Filters;
using MongoDB.Driver;
using MongoService;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Collections
{
    public class UserStoryCollection : ICollection<UserStory>
    {
        protected override string CollectionName => "user_story";

        public override void Add(ref UserStory document)
        {
            base.Add(ref document);

            this.Collection.InsertOne(document);
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<UserStory>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public override void Edit(ref UserStory document)
        {
            base.Edit(ref document);

            var filter = Builders<UserStory>.Filter.Eq("_id", document.Id);
            this.Collection.ReplaceOne(filter, document);
        }

        public override bool Exists(ref UserStory document, bool updateValues = true)
        {
            long userId = document.UserID;
            long animeId = document.AnimeID;

            UserStory reference = this.Collection.Find(x => x.UserID == userId && x.AnimeID == animeId).FirstOrDefault();

            if(reference != null)
            {
                if (updateValues)
                {
                    document.Id = reference.Id;
                    document.CreationDate = reference.CreationDate;
                    document.UpdateDate = reference.UpdateDate;
                    document.Synced = reference.Synced;
                }

                return true;
            }

            return false;
        }

        public override UserStory Get(long id)
        {
            return this.Collection.Find(x => x.Id == id).FirstOrDefault();
        }

        public override Paging<UserStory> GetList<TFilter>(IFilter<TFilter> filter)
        {
            UserStoryFilter userStoryFilter = filter as UserStoryFilter;

            var builder = Builders<UserStory>.Filter;
            FilterDefinition<UserStory> queryFilter = builder.Empty;

            userStoryFilter.ApplyBaseFilter(builder, ref queryFilter);

            if (userStoryFilter.user_id != null)
            {
                queryFilter &= builder.Eq("user_id", userStoryFilter.user_id);
            }

            if (userStoryFilter.anime_id != null)
            {
                queryFilter &= builder.Eq("anime_id", userStoryFilter.anime_id);
            }

            if(userStoryFilter.status != null)
            {
                queryFilter &= builder.Eq("status", userStoryFilter.status);
            }

            if(userStoryFilter.synced != null)
            {
                queryFilter &= builder.Eq("synced", userStoryFilter.synced);
            }

            SortDefinition<UserStory> sort = userStoryFilter.ApplySort<UserStory>(
                new System.Collections.Generic.List<string>
                {
                    "creation_date"
                },
                new System.Collections.Generic.List<short>
                {
                    -1
                });

            return new Paging<UserStory>(this.Collection, userStoryFilter.page, queryFilter, sort, userStoryFilter.per_page);
        }
    }
}
