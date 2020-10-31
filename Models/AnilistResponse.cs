using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class AnilistResponse
    {
        public ResponsePage Page { get; set; }

        public class ResponsePage
        {
            public PageInfo pageInfo { get; set; }
            public List<Media> media { get; set; }
        }

        public class PageInfo
        {
            public int lastPage { get; set; }
        }

        public class Media
        {
            public int id { get; set; }
            public int idMal { get; set; }
            public AnimeFormatEnum format { get; set; }
            public AnimeStatusEnum status { get; set; }
            public MediaDate startDate { get; set; }
            public MediaDate endDate { get; set; }
            public MediaTitle title { get; set; }
            public string description { get; set; }
        }

        public class MediaTitle
        {
            public string romaji { get; set; }
            public string native { get; set; }
        }

        public class MediaDate
        {
            public int? year { get; set; }
            public int? month { get; set; }
            public int? day { get; set; }
        }
    }
}
