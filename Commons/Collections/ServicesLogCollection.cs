using MongoDB.Driver;
using MongoService;
using System;

namespace Commons.Collections
{
    public class ServicesLogCollection : ICollection<ServicesLog>
    {
        protected override string CollectionName => "service_log";

        public override void Add(ref ServicesLog document)
        {
            document.Id = this.CalcNewId();
            document.CreationDate = DateTime.Now;
            document.UpdateDate = null;

            this.Collection.InsertOne(document);
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<ServicesLog>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public override void Edit(ref ServicesLog document)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(ref ServicesLog document, bool updateValues = true)
        {
            throw new NotImplementedException();
        }

        public override ServicesLog Get(long id)
        {
            throw new NotImplementedException();
        }

        public override Paging<ServicesLog> GetList<TFilter>(IFilter<TFilter> filter)
        {
            throw new NotImplementedException();
        }
    }
}
