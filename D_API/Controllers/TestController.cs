using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Controllers
{
    [ApiController]
    [Route("api/test")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TestController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("probe")]
        public IActionResult Probe() => Ok();

        [HttpGet("probeAuth")]
        public IActionResult ProbeAuth() => Ok();

        [Authorize(Roles = Roles.Moderator)]
        [HttpGet("probeAuthMod")]
        public IActionResult ProbeAuthMod() => Ok();

        [Authorize(Roles = Roles.Administrator)]
        [HttpGet("probeAuthAdmin")]
        public IActionResult ProbeAuthAdmin() => Ok();

        [Authorize(Roles = Roles.Root)]
        [HttpGet("probeAuthRoot")]
        public IActionResult ProbeAuthRoot() => Ok();

        [AllowAnonymous]
        [HttpGet("probeRole")]
        public IActionResult ProbeRole()
            =>    User.Identity?.IsAuthenticated is null or false ? Ok(-1)
                : User.IsInRole("root") ? Ok(3)
                : User.IsInRole("admin") ? Ok(2)
                : User.IsInRole("mod") ? Ok(1)
                : Ok(0);
    }
}
