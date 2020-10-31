using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MongoService
{
    public abstract class IFilter<T>
    {
        private string _locale = LocalizationEnum.English;
        public string Locale { get { return this._locale; } set { this._locale = value; } }
    }
}
