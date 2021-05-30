using MongoDB.Driver;
using MongoService;
using System;

namespace Commons.Collections
{
    public class ServicesStatusCollection : ICollection<ServicesStatus>
    {
        protected override string CollectionName => "service_status";

        public override void Add(ref ServicesStatus document)
        {
            base.Add(ref document);

            this.Collection.InsertOne(document);
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<ServicesStatus>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public override void Edit(ref ServicesStatus document)
        {
            base.Edit(ref document);

            var filter = Builders<ServicesStatus>.Filter.Eq("_id", document.Id);
            this.Collection.ReplaceOne(filter, document);
        }

        public override bool Exists(ref ServicesStatus document, bool updateValues = true)
        {
            string name = document.Name;
            ServicesStatus reference = this.Collection.Find(x => x.Name == name).FirstOrDefault();

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

        public override ServicesStatus Get(long id)
        {
            throw new NotImplementedException();
        }

        public override Paging<ServicesStatus> GetList<TFilter>(IFilter<TFilter> filter)
        {
            throw new NotImplementedException();
        }
    }
}
