using System.Net;

namespace Commons
{
    public class APIResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public string Version { get; set; }

        public APIResponse(HttpStatusCode statusCode,string message = "", object data = null, string version = "1")
        {
            this.StatusCode = statusCode;
            this.Message = message;
            this.Data = data;
            this.Version = version;
        }

        public APIResponse()
        {
            this.StatusCode = HttpStatusCode.InternalServerError;
            this.Message = "Something bad happened";
            this.Data = null;
            this.Version = "1";
        }
    }
}
