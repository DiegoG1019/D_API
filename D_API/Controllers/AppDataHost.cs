using D_API.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiegoG.Utilities.IO;
using System.IO;
using D_API.Types;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace D_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/appdatahost")]
    public class AppDataHost : ControllerBase
    {
        private readonly IAppDataAccessKeeper Keeper;

        public AppDataHost(IAppDataAccessKeeper keeper)
        {
            Keeper = keeper;
        }

        static AppDataHost()
        {
            Directory.CreateDirectory(Directories.AppDataHost);
        }

        [HttpGet("config/{appname}")]
        public async Task<IActionResult> GetConfig(string appname)
        {
            var s = Directories.InAppDataHost(appname);
            return System.IO.File.Exists(s) ? await Keeper.CheckAccess(User, s) ? Ok(await ReadFile(s)) : Forbid() : NotFound();
        }

        [HttpPost("config/{appname}")]
        public Task<IActionResult> WriteConfig(string appname, [FromBody]string body) => WriteConfig(appname, body, false);

        [HttpPost("config/{appname}/{ow}")]
        public async Task<IActionResult> WriteConfig(string appname, [FromBody]string body, bool ow)
        {
            var file = Directories.InAppDataHost(appname);

            if (!await Keeper.CheckAccess(User, file))
                return Forbid();

            await Keeper.NewFile(User, file);

            if (!ow && System.IO.File.Exists(file))
                return Unauthorized("File already exists, cannot assume overwrite. If you wish to overwrite it, please use \"config/{appname}/true\"");
            await WriteFile(file, body);
            return Ok();
        }

        private static async Task<string> ReadFile(string file)
        {
            using StreamReader InFile = new(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read));
            return await InFile.ReadToEndAsync();
        }

        private static async Task WriteFile(string file, string data)
        {
            using StreamWriter OutFile = new(new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read));
            await OutFile.WriteLineAsync(data);
        }
    }
}