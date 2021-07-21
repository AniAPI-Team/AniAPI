using MongoDB.Driver;
using ServiceMongo.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService
{
    /// <summary>
    /// Base class to create a MongoDB query filter
    /// </summary>
    /// <typeparam name="TDocument">IModel derived base class</typeparam>
    public abstract class IFilter<TDocument>
    {
        private string _locale = "en";
        /// <summary>
        /// ISO 639-1 i18n code
        /// </summary>
        public string locale { get { return this._locale; } set { this._locale = value; } }

        private int _page = 1;
        /// <summary>
        /// Number of page
        /// </summary>
        public int page { get { return this._page; } set { this._page = value; } }

        private int _per_page = 100;
        public int per_page { get { return this._per_page; } set { this._per_page = value; } }

        [CommaSeparated]
        public List<long> ids { get; set; } = new List<long>();

        [CommaSeparated]
        public List<string> sort_fields { get; set; } = new List<string>();

        [CommaSeparated]
        public List<short> sort_directions { get; set; } = new List<short>();

        public FilterDefinition<XDocument> ApplyBaseFilter<XDocument>(FilterDefinitionBuilder<XDocument> builder, ref FilterDefinition<XDocument> filter)
        {
            if (ids.Count > 0)
            {
                filter &= builder.AnyIn("_id", ids);
            }

            return filter;
        }

        public SortDefinition<XDocument> ApplySort<XDocument>(List<string> fallbackFields, List<short> fallbackDirs)
        {
            SortDefinition<XDocument> sort = null;

            if(sort_fields.Count == 0)
            {
                sort_fields = fallbackFields;
                sort_directions = fallbackDirs;
            }

            if (sort_fields.Count > 0)
            {
                for (int i = 0; i < sort_fields.Count; i++)
                {
                    string field = sort_fields[i];
                    short dir = (sort_directions.Count - 1) >= i ? sort_directions[i] : (short)1;

                    if(field == "id")
                    {
                        field = "_id";
                    }

                    if(sort == null)
                    {
                        switch (dir)
                        {
                            case 1:
                                sort = Builders<XDocument>.Sort.Ascending(field);
                                break;
                            case -1:
                                sort = Builders<XDocument>.Sort.Descending(field);
                                break;
                        }
                    }
                    else
                    {
                        switch (dir)
                        {
                            case 1:
                                sort = sort.Ascending(field);
                                break;
                            case -1:
                                sort = sort.Descending(field);
                                break;
                        }
                    }
                }
            }

            return sort;
        }
    }
}
