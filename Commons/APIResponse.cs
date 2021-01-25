using System.Net;

namespace Commons
{
    public static class ApiResponseManager
    {

        public static APIResponse CreateSuccessResponse(object _result = null,
            string _apiVersion = "1.0.0.0")
        {
            APIResponse response = new APIResponse();

            response.statusCode = HttpStatusCode.OK;
            response.message = null;
            response.result = _result;
            response.apiVersion = _apiVersion;

            return response;
        }

        public static APIResponse CreateErrorResponse(HttpStatusCode _statusCode,
            string _message,
            object _result = null,
            //ApiException _apiError = null,
            string _apiVersion = "1.0.0.0")
        {
            APIResponse response = new APIResponse();

            response.statusCode = _statusCode;
            response.message = _message;
            response.result = _result;
            response.apiVersion = _apiVersion;

            return response;
        }
    }
    public class APIResponse
    {
        public APIResponse(HttpStatusCode _statusCode,
            string _message = "",
            object _result = null,
            //ApiException _apiError = null,
            string _apiVersion = "1.0.0.0")
        {
            this.statusCode = _statusCode;
            this.message = _message;
            this.result = _result;
            //this.apiError = _apiError;
            this.apiVersion = _apiVersion;
        }

        public APIResponse()
        {
            this.statusCode = HttpStatusCode.InternalServerError;
            this.message = "";
            this.result = null;
            //this.apiError = null;
            this.apiVersion = "1.0.0.0";
        }

        public HttpStatusCode statusCode { get; set; }
        public string message { get; set; }
        public object result { get; set; }
        public string apiVersion { get; set; }
    }
}
