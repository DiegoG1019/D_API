using D_API.Types.Responses;
using DiegoG.Utilities.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace D_API.Controllers
{
    public class ResponseResult : ObjectResult
    {
        public ResponseResult(object value) : base(value) { }
    }

    public abstract class D_APIController : Controller
    {
        [NonAction]
        public ResponseResult ResponseResult([ActionResultStatusCode] HttpStatusCode code, [ActionResultObjectValue] BaseResponse response) 
            => new(response)
            {
                StatusCode = (int)code
            };

        [NonAction]
        public ResponseResult Forbidden(BaseResponse response) => ResponseResult(HttpStatusCode.Forbidden, response);

        [NonAction]
        public ResponseResult BadRequest(BaseResponse response) => ResponseResult(HttpStatusCode.BadRequest, response);

        [NonAction]
        public ResponseResult Ok(BaseResponse response) => ResponseResult(HttpStatusCode.OK, response);

        [NonAction]
        public ResponseResult Unauthorized(BaseResponse response) => ResponseResult(HttpStatusCode.Unauthorized, response);

        [NonAction]
        public ResponseResult NotFound(BaseResponse response) => ResponseResult(HttpStatusCode.NotFound, response);

        [Route("/error")]
        public virtual ResponseResult Error(string title = "Unknown Error", string errorType = "", string message = "") 
            => ResponseResult(HttpStatusCode.InternalServerError, new UnspecifiedError(title, errorType, message));

        [NonAction]
        public virtual ResponseResult Error(Exception exception, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string? title = null, bool useExceptionMessage = false)
            => ResponseResult(statusCode, 
                              new UnspecifiedError(
                                                   title ?? "An error ocurred while processing your request", 
                                                   exception.GetType().Name,
                                                   useExceptionMessage ? exception.Message : "A Report has been sent to the server"));
    }
}
