using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Commons.Resources
{
    public class LocalizationResource
    {
        [JsonPropertyName("localizations")]
        public List<Localization> Localizations { get; set; }

        public class Localization
        {
            public string i18n { get; set; }

            [JsonPropertyName("label")]
            public string Label { get; set; }
        }
    }
}
