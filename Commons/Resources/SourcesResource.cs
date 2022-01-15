using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Commons.Resources
{
    public class SourcesResource
    {
        [JsonPropertyName("sources")]
        public List<Source> Sources { get; set; }
    }

    public class Source
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("i18n")]
        public string i18n { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }
    }
}
