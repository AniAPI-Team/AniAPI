using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public static class ApiResponse
    {
        public static JsonResult Success(HttpStatusCode statusCode, object data)
        {
            return new JsonResult(new SuccessResponse()
            {
                Status = statusCode,
                Data = data
            });
        }

        public static JsonResult Error(ApiException exception)
        {
            return new JsonResult(new ErrorResponse()
            {
                Status = exception.StatusCode,
                Message = exception.Message,
                Description = exception.Description
            });
        }

        public static JsonResult Error(Exception exception)
        {
            return new JsonResult(new ErrorResponse()
            {
                Status = HttpStatusCode.InternalServerError,
                Message = exception.Message,
                Description = null
            });
        }

        public class SuccessResponse
        {
            public HttpStatusCode Status { get; set; }
            public object Data { get; set; }
        }

        public class ErrorResponse
        {
            public HttpStatusCode Status { get; set; }
            public string Message { get; set; }
            public string Description { get; set; }
        }
    }
}
