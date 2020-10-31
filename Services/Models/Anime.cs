using Models;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService.Models
{
    public class Anime : IModel
    {
        public Anime() { }

        public Anime(AnilistResponse.Media media)
        {
            this.Titles = new Dictionary<string, string>();
            this.Titles[LocalizationEnum.English] = media.title.romaji;
            this.Titles[LocalizationEnum.Japanese] = media.title.native;

            this.Descriptions = new Dictionary<string, string>();
            this.Descriptions[LocalizationEnum.English] = media.description;

            this.AnilistId = media.id;
            this.MyAnimeListId = media.idMal;
            this.Format = media.format;
            this.Status = media.status;

            if (media.startDate.year.HasValue)
            {
                this.StartDate = new DateTime(media.startDate.year.Value, media.startDate.month.Value, media.startDate.day.Value);
            }

            if (media.endDate.year.HasValue)
            {
                this.EndDate = new DateTime(media.endDate.year.Value, media.endDate.month.Value, media.endDate.day.Value);
            }
        }

        [BsonElement("anilist_id")]
        public int AnilistId { get; set; }

        [BsonElement("mal_id")]
        public int MyAnimeListId { get; set; }

        [BsonElement("format")]
        public AnimeFormatEnum Format { get; set; }

        [BsonElement("status")]
        public AnimeStatusEnum Status { get; set; }

        [BsonElement("titles")]
        public Dictionary<string, string> Titles { get; set; }

        [BsonElement("descriptions")]
        public Dictionary<string, string> Descriptions { get; set; }

        [BsonElement("start_date")]
        public DateTime? StartDate { get; set; }

        [BsonElement("end_date")]
        public DateTime? EndDate { get; set; }
    }
}
