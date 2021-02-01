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
            document.Id = this.CalcNewId();
            document.CreationDate = DateTime.Now;
            document.UpdateDate = null;

            this.Collection.InsertOne(document);
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<User>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public override void Edit(ref User document)
        {
            document.UpdateDate = DateTime.Now;

            var filter = Builders<User>.Filter.Eq("_id", document.Id);
            this.Collection.ReplaceOne(filter, document);
        }

        public override bool Exists(ref User document, bool updateValues = true)
        {
            string username = document.Username;
            User reference = this.Collection.Find(x => x.Username == username).FirstOrDefault();

            if (reference != null && updateValues)
            {
                document.Id = reference.Id;
                document.CreationDate = reference.CreationDate;
                document.UpdateDate = reference.UpdateDate;
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

            if (!string.IsNullOrEmpty(userFilter.username))
            {
                queryFilter = queryFilter & builder.Regex($"username", new BsonRegularExpression($".*{userFilter.username}.*"));
            }

            if (!string.IsNullOrEmpty(userFilter.email))
            {
                queryFilter = queryFilter & builder.Regex($"email", new BsonRegularExpression($".*{userFilter.email}.*"));
            }

            return new Paging<User>(this.Collection, userFilter.page, queryFilter);
        }
    }
}
