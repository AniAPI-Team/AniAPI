using Commons.Filters;
using MongoDB.Driver;
using MongoService;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Collections
{
    public class OAuthClientCollection : ICollection<OAuthClient>
    {
        protected override string CollectionName => "oauth_client";

        public override void Add(ref OAuthClient document)
        {
            base.Add(ref document);

            do
            {
                document.ClientID = Guid.NewGuid();
            }
            while (this.Exists(ref document, false) == true);

            this.Collection.InsertOne(document);
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<OAuthClient>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            this.Collection.DeleteOne(x => x.Id == id);
        }

        public override void Edit(ref OAuthClient document)
        {
            base.Edit(ref document);

            var filter = Builders<OAuthClient>.Filter.Eq("_id", document.Id);
            this.Collection.ReplaceOne(filter, document);
        }

        public override bool Exists(ref OAuthClient document, bool updateValues = true)
        {
            Guid clientId = document.ClientID;

            OAuthClient reference = this.Collection.Find(x => x.ClientID == clientId).FirstOrDefault();

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

        public override OAuthClient Get(long id)
        {
            return this.Collection.Find(x => x.Id == id).FirstOrDefault();
        }

        public override Paging<OAuthClient> GetList<TFilter>(IFilter<TFilter> filter)
        {
            OAuthClientFilter oAuthClientFilter = filter as OAuthClientFilter;

            var builder = Builders<OAuthClient>.Filter;
            FilterDefinition<OAuthClient> queryFilter = builder.Empty;

            oAuthClientFilter.ApplyBaseFilter(builder, ref queryFilter);

            if (oAuthClientFilter.user_id != null)
            {
                queryFilter &= builder.Eq("user_id", oAuthClientFilter.user_id);
            }

            if(oAuthClientFilter.client_id != null)
            {
                queryFilter &= builder.Eq("client_id", oAuthClientFilter.client_id);
            }

            SortDefinition<OAuthClient> sort = oAuthClientFilter.ApplySort<OAuthClient>(
                new System.Collections.Generic.List<string>
                {
                    "_id"
                },
                new System.Collections.Generic.List<short>
                {
                    1
                });

            return new Paging<OAuthClient>(this.Collection, oAuthClientFilter.page, queryFilter, sort, oAuthClientFilter.per_page);
        }
    }
}
