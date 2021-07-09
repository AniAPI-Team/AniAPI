using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;

namespace MongoService
{
    /// <summary>
    /// List of IModel MongoDB documents
    /// </summary>
    /// <typeparam name="TDocument">IModel derived child class</typeparam>
    public class Paging<TDocument>
    {
        private int _documentsPerPage;

        /// <summary>
        /// The current list page
        /// </summary>
        [JsonPropertyName("current_page")]
        [JsonProperty(PropertyName = "current_page")]
        public int CurrentPage { get; set; }

        /// <summary>
        /// The list documents count
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// The current page documents
        /// </summary>
        public List<TDocument> Documents { get; set; }

        private int _skip => (this.CurrentPage - 1) * this._documentsPerPage;
        private int _limit => this._documentsPerPage;

        /// <summary>
        /// The last page available
        /// </summary>
        [JsonPropertyName("last_page")]
        [JsonProperty(PropertyName = "last_page")]
        public int LastPage => (int)Math.Ceiling((double)this.Count / this._documentsPerPage);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection">A MongoDB collection reference</param>
        /// <param name="page">A page number</param>
        /// <param name="filter">A MongoDB query filter</param>
        public Paging(IMongoCollection<TDocument> collection, int page, FilterDefinition<TDocument> filter, SortDefinition<TDocument> sort, int documentsPerPage = 100)
        {
            if(documentsPerPage > 100)
            {
                documentsPerPage = 100;
            }

            this._documentsPerPage = documentsPerPage;
            this.CurrentPage = page;
            this.Count = collection.CountDocuments(filter);
            this.Documents = collection.Find(filter).Sort(sort).Skip(this._skip).Limit(this._limit).ToList();
        }
    }
}
