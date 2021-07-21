using Commons.Filters;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoService;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Collections
{
    public class UserCollection : ICollection<User>
    {
        protected override string CollectionName => "user";

        public override void Add(ref User document)
        {
            base.Add(ref document);

            this.Collection.InsertOne(document);
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<User>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            this.Collection.DeleteOne(x => x.Id == id);
        }

        public override void Edit(ref User document)
        {
            base.Edit(ref document);

            var filter = Builders<User>.Filter.Eq("_id", document.Id);
            this.Collection.ReplaceOne(filter, document);
        }

        public override bool Exists(ref User document, bool updateValues = true)
        {
            string email = document.Email;
            User reference = null;

            if (!string.IsNullOrEmpty(email))
            {
                reference = this.Collection.Find(x => x.Email == email).FirstOrDefault();
            }
            else
            {
                long userId = document.Id;
                reference = this.Collection.Find(x => x.Id == userId).FirstOrDefault();
            }

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

        public override User Get(long id)
        {
            return this.Collection.Find(x => x.Id == id).FirstOrDefault();
        }

        public override Paging<User> GetList<TFilter>(IFilter<TFilter> filter)
        {
            UserFilter userFilter = filter as UserFilter;

            var builder = Builders<User>.Filter;
            FilterDefinition<User> queryFilter = builder.Empty;

            userFilter.ApplyBaseFilter(builder, ref queryFilter);

            if (!string.IsNullOrEmpty(userFilter.username))
            {
                queryFilter = queryFilter & builder.Regex($"username", new BsonRegularExpression($".*{userFilter.username}.*", "i"));
            }

            if (!string.IsNullOrEmpty(userFilter.email))
            {
                queryFilter = queryFilter & builder.Eq($"email", userFilter.email);
            }

            SortDefinition<User> sort = userFilter.ApplySort<User>(
                new System.Collections.Generic.List<string>
                {
                    "username"
                },
                new System.Collections.Generic.List<short>
                {
                    1
                });

            return new Paging<User>(this.Collection, userFilter.page, queryFilter, sort, userFilter.per_page);
        }
    }
}
