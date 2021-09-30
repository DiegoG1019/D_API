using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Controllers
{
    public abstract class D_APIController : Controller
    {
        [NonAction]
        public ObjectResult Forbidden(string title, string? detail = null)
               => Problem(type: "/docs/errors/forbidden",
                   title: title,
                   detail: detail,
                   statusCode: StatusCodes.Status403Forbidden,
                   instance: HttpContext.Request.Path);
    }
}
