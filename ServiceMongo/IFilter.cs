using MongoDB.Driver;
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

        public List<long> ids { get; set; } = new List<long>();

        public List<string> sorts { get; set; } = new List<string>();

        public FilterDefinition<XDocument> ApplyBaseFilter<XDocument>(FilterDefinitionBuilder<XDocument> builder, ref FilterDefinition<XDocument> filter)
        {
            if (ids.Count > 0)
            {
                filter &= builder.AnyIn("_id", ids);
            }



            return filter;
        }
    }
}
