using D_API.Types.Responses;
using DiegoG.Utilities.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace D_API.Controllers
{
    public abstract class D_APIController : Controller
    {
        [NonAction]
        protected ObjectResult Result(HttpStatusCode code, BaseResponse response) => StatusCode((int)code, response);

        [NonAction]
        public ObjectResult Forbidden(BaseResponse response) => Result(HttpStatusCode.Forbidden, response);

        [NonAction]
        public ObjectResult BadRequest(BaseResponse response) => Result(HttpStatusCode.BadRequest, response);

        [NonAction]
        public ObjectResult OK(BaseResponse response) => Result(HttpStatusCode.OK, response);

        [NonAction]
        public ObjectResult Unauthorized(BaseResponse response) => Result(HttpStatusCode.Unauthorized, response);

        [NonAction]
        public ObjectResult NotFound(BaseResponse response) => Result(HttpStatusCode.NotFound, response);
    }
}
