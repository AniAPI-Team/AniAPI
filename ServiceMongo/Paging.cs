using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MongoService
{
    public class Paging<T>
    {
        private int _documentsPerPage = 50;

        public int CurrentPage { get; set; }

        public long Count { get; set; }

        public List<T> Documents { get; set; }

        private int _skip => (this.CurrentPage - 1) * this._documentsPerPage;
        private int _limit => this._documentsPerPage;
        public int LastPage => (int)Math.Ceiling((double)this.Count / this._documentsPerPage);

        public Paging(IMongoCollection<T> collection, int page, FilterDefinition<T> filter)
        {
            this.CurrentPage = page;
            this.Count = collection.CountDocuments(filter);
            this.Documents = collection.Find(filter).Skip(this._skip).Limit(this._limit).ToList();
        }
    }
}
