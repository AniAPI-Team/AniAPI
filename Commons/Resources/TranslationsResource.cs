using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Commons.Resources
{
    public class TranslationsResource
    {
        [JsonPropertyName("translations")]
        public List<Translation> Translations { get; set; }

        public class Translation
        {
            [JsonPropertyName("key")]
            public string Key { get; set; }

            [JsonPropertyName("i18n")]
            public Dictionary<string, string> i18n { get; set; }
        }
    }
}
