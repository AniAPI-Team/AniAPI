using MongoDB.Driver;
using MongoService;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Collections
{
    public class AnimeSongCollection : ICollection<AnimeSong>
    {
        protected override string CollectionName => "anime_song";

        public override void Add(ref AnimeSong document)
        {
            base.Add(ref document);

            this.Collection.InsertOne(document);
        }

        public override long Count()
        {
            return this.Collection.CountDocuments(Builders<AnimeSong>.Filter.Empty);
        }

        public override void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public override void Edit(ref AnimeSong document)
        {
            base.Edit(ref document);

            var filter = Builders<AnimeSong>.Filter.Eq("_id", document.Id);
            this.Collection.ReplaceOne(filter, document);
        }

        public override bool Exists(ref AnimeSong document, bool updateValues = true)
        {
            long animeID = document.AnimeID;
            string title = document.Title;
            string artist = document.Artist;

            AnimeSong reference = this.Collection.Find(x => x.Title == title && x.Artist == artist && x.AnimeID == animeID).FirstOrDefault();

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

        public override AnimeSong Get(long id)
        {
            throw new NotImplementedException();
        }

        public override Paging<AnimeSong> GetList<TFilter>(IFilter<TFilter> filter)
        {
            throw new NotImplementedException();
        }
    }
}
