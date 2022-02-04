using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Commons
{
    public class APIException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public object Description { get; set; }
        public string Version { get; set; }

        public APIException(HttpStatusCode statusCode, string message, object description, string version = "1")
        {
            this.StatusCode = statusCode;
            this.Message = message;
            this.Description = description;
            this.Version = version;
        }
    }
}
