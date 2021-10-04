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
        public ObjectResult Forbidden(BaseResponse response) => ResponseResult(HttpStatusCode.Forbidden, response);

        [NonAction]
        public ObjectResult BadRequest(BaseResponse response) => ResponseResult(HttpStatusCode.BadRequest, response);

        [NonAction]
        public ObjectResult Ok(BaseResponse response) => ResponseResult(HttpStatusCode.OK, response);

        [NonAction]
        public ObjectResult Unauthorized(BaseResponse response) => ResponseResult(HttpStatusCode.Unauthorized, response);

        [NonAction]
        public ObjectResult NotFound(BaseResponse response) => ResponseResult(HttpStatusCode.NotFound, response);
    }
}
