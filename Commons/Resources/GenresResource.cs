using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Commons.Resources
{
    public class GenresResource
    {
        [JsonPropertyName("genres")]
        public List<string> Genres { get; set; }
    }
}
