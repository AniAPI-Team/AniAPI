using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService
{
    public abstract class IFilter<T>
    {
        private string _locale = "en";
        public string Locale { get { return this._locale; } set { this._locale = value; } }

        private int _page = 1;
        public int Page { get { return this._page; } set { this._page = value; } }
    }
}
