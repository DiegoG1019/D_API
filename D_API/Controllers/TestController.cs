using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
    }
}
