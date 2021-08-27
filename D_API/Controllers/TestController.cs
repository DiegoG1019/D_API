﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        [Authorize(Roles = "mod")]
        [HttpGet("probeAuthMod")]
        public IActionResult ProbeAuthMod() => Ok();

        [Authorize(Roles = "admin")]
        [HttpGet("probeAuthAdmin")]
        public IActionResult ProbeAuthAdmin() => Ok();

        [Authorize(Roles = "root")]
        [HttpGet("probeAuthRoot")]
        public IActionResult ProbeAuthRoot() => Ok();
    }
}
