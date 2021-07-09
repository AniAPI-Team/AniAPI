using Commons.Filters;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using MongoService;

namespace Commons.Collections
{
    public class WebsiteCollection : ICollection<Website>
    {
        protected override string CollectionName => "website";

        public override void Add(ref Website document)
        {
            base.Add(ref document);

            this.Collection.InsertOne(document);
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<Website>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public override void Edit(ref Website document)
        {
            base.Edit(ref document);
            
            var filter = Builders<Website>.Filter.Eq("_id", document.Id);
            this.Collection.ReplaceOne(filter, document);
        }

        public override Website Get(long id)
        {
            return this.Collection.Find(x => x.Id == id).FirstOrDefault();
        }

        public override bool Exists(ref Website document, bool updateValues = true)
        {
            string name = document.Name;
            Website reference = this.Collection.Find(x => x.Name == name).FirstOrDefault();

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

        public override Paging<Website> GetList<TFilter>(IFilter<TFilter> filter)
        {
            WebsiteFilter websiteFilter = filter as WebsiteFilter;

            var builder = Builders<Website>.Filter;
            FilterDefinition<Website> queryFilter = builder.Empty;

            if (!string.IsNullOrEmpty(websiteFilter.name))
            {
                queryFilter = queryFilter & builder.Regex($"name", new BsonRegularExpression($".*{websiteFilter.name}.*", "i"));
            }

            SortDefinition<Website> sort = Builders<Website>.Sort.Ascending(x => x.Id);

            return new Paging<Website>(this.Collection, websiteFilter.page, queryFilter, sort, websiteFilter.per_page);
        }
    }
}
