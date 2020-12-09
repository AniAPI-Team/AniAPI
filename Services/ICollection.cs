using MongoDB.Bson;
using MongoDB.Driver;
using MongoService.Helpers;
using MongoService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService
{
    public abstract class ICollection<TDocument>
    {
        protected abstract string CollectionName { get; }
        public IMongoCollection<TDocument> Collection => Connection.Instance.Database.GetCollection<TDocument>(this.CollectionName);
        public abstract void Add(ref TDocument document);
        public abstract long Count();
        public abstract void Delete(long id);
        public abstract void Edit(ref TDocument document);
        public abstract bool Exists(ref TDocument document);
        public abstract TDocument Get(long id);
        public abstract Paging<TDocument> GetList<TFilter>(IFilter<TFilter> filter);
        protected long CalcNewId()
        {
            if (this.Count() == 0)
            {
                return 1;
            }
            else
            {
                IModel document = this.Collection.Find(Builders<TDocument>.Filter.Empty).SortByDescending(x => (x as IModel).Id).Limit(1).FirstOrDefault() as IModel;
                return document.Id + 1;
            }
        }
    }
}
