using Commons.Filters;
using MongoDB.Bson;
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
            return this.Collection.Find(x => x.Id == id).FirstOrDefault();
        }

        public override Paging<AnimeSong> GetList<TFilter>(IFilter<TFilter> filter)
        {
            AnimeSongFilter songFilter = filter as AnimeSongFilter;

            var builder = Builders<AnimeSong>.Filter;
            FilterDefinition<AnimeSong> queryFilter = builder.Empty;

            songFilter.ApplyBaseFilter(builder, ref queryFilter);

            if (songFilter.anime_id != null)
            {
                queryFilter &= builder.Eq("anime_id", songFilter.anime_id);
            }

            if (!string.IsNullOrEmpty(songFilter.title))
            {
                queryFilter &= builder.Regex("title", new BsonRegularExpression($".*{songFilter.title}.*", "i"));
            }

            if (!string.IsNullOrEmpty(songFilter.artist))
            {
                queryFilter &= builder.Regex("artist", new BsonRegularExpression($".*{songFilter.artist}.*", "i"));
            }

            if(songFilter.year != null)
            {
                queryFilter &= builder.Eq("year", songFilter.year);
            }

            if(songFilter.season != null)
            {
                queryFilter &= builder.Eq("season", songFilter.season);
            }

            if(songFilter.type != null)
            {
                queryFilter &= builder.Eq("type", songFilter.type);
            }

            SortDefinition<AnimeSong> sort = songFilter.ApplySort<AnimeSong>(
                new System.Collections.Generic.List<string>
                {
                    "year",
                    "season"
                },
                new System.Collections.Generic.List<short>
                {
                    -1,
                    -1
                });

            return new Paging<AnimeSong>(this.Collection, songFilter.page, queryFilter, sort, songFilter.per_page);
        }
    }
}
