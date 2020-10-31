using MongoDB.Driver;
using MongoService.Helpers;
using MongoService.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace MongoService
{
    public class AnimeCollection : ICollection<Anime>
    {
        protected override string CollectionName => "anime";

        public override void Add(ref Anime document)
        {
            document.Id = this.CalcNewId();
            document.CreationDate = DateTime.Now;
            document.UpdateDate = null;

            this.Collection.InsertOne(document);
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<Anime>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public override void Edit(long id, Anime document)
        {
            throw new NotImplementedException();
        }

        public override Anime Get(long id)
        {
            return this.Collection.Find(x => x.Id == id).FirstOrDefault();
        }

        public override List<Anime> GetList<TFilter>(IFilter<TFilter> filter)
        {
            AnimeFilter animeFilter = filter as AnimeFilter;

            return this.Collection.Find(x => x.Titles[animeFilter.Locale].Contains(animeFilter.Title)).ToList();
        }
    }
}
