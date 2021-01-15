using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }

        public ApiException(HttpStatusCode statusCode, string message, string description)
        {
            this.StatusCode = statusCode;
            this.Message = message;
            this.Description = description;
        }
    }
}
