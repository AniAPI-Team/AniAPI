using MongoDB.Bson;
using MongoDB.Driver;
using MongoService.Helpers;
using Polly;
using ServiceMongo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoService
{
    /// <summary>
    /// Base class to create a MongoDB collection
    /// </summary>
    /// <typeparam name="TDocument">IModel derived child class</typeparam>
    public abstract class ICollection<TDocument> where TDocument : IModel
    {
        /// <summary>
        /// MongoDB collection name
        /// </summary>
        protected abstract string CollectionName { get; }

        /// <summary>
        /// MongoDB collection reference to interact with
        /// </summary>
        public IMongoCollection<TDocument> Collection => Connection.Instance.Database.GetCollection<TDocument>(this.CollectionName);

        /// <summary>
        /// Add a new document in the collection
        /// </summary>
        /// <param name="document">The document object</param>
        public virtual void Add(ref TDocument document)
        {
            document.Id = this.CalcNewId();
            document.CreationDate = DateTime.Now;
            document.UpdateDate = null;
        }

        /// <summary>
        /// Count all the documents inside the collection
        /// </summary>
        /// <returns></returns>
        public abstract long Count();

        /// <summary>
        /// Delete a document from the collection
        /// </summary>
        /// <param name="id">The document id</param>
        public abstract void Delete(long id);

        /// <summary>
        /// Edit a document already inside the collection
        /// </summary>
        /// <param name="document">The document object</param>
        public virtual void Edit(ref TDocument document)
        {
            document.UpdateDate = DateTime.Now;
        }

        /// <summary>
        /// Check if a document already exist in the collection
        /// </summary>
        /// <param name="document">The document object</param>
        /// <param name="updateValues">Default to true, set it to false if you don't want to rebase the document properties</param>
        /// <returns></returns>
        public abstract bool Exists(ref TDocument document, bool updateValues = true);

        /// <summary>
        /// Get a document from the collection
        /// </summary>
        /// <param name="id">The document id</param>
        /// <returns></returns>
        public abstract TDocument Get(long id);

        /// <summary>
        /// Get a list of documents from the collection
        /// </summary>
        /// <typeparam name="TFilter">IFilter derived child class</typeparam>
        /// <param name="filter">The filter object</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the last document created in the collection
        /// </summary>
        /// <returns></returns>
        public TDocument Last()
        {
            return this.Collection.Find(Builders<TDocument>.Filter.Empty).SortByDescending(x => (x as IModel).Id).Limit(1).FirstOrDefault();
        }

        public void TryInsertWithRetry(ref TDocument document, int retryTimes = 3, int retryDelay = 2)
        {
            bool hasInserted = false;
            int times = 1;

            do
            {
                try
                {
                    this.Add(ref document);
                    hasInserted = true;
                }
                catch
                {
                    hasInserted = false;
                    times++;

                    Thread.Sleep(retryDelay * 1000);
                }
            }
            while (!hasInserted || times > retryTimes);
            //TimeSpan[] retries = new TimeSpan[retryTimes];

            //for(int i = 0; i < retryTimes; i++)
            //{
            //    int s = 1 + (i == 0 ? 0 : (int)Math.Pow(retryDelay, i));
            //    retries[i] = TimeSpan.FromSeconds(s);
            //}
            //
            //var policy = Policy.Handle<ConcurrencyException>()
            //    .WaitAndRetryAsync(retries);
            //
            //return await policy.ExecuteAsync(() =>
            //{
            //    this.Add(ref document);
            //
            //    return Task.FromResult(document);
            //});
        }
    }
}
