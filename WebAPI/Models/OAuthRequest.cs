using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class OAuthRequest
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectURI { get; set; }
        public string ResponseType { get; set; }
        public string State { get; set; }
    }
}
