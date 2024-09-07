using Implementation.Helper;

using IntegratedImplementation.Interfaces.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntegratedDigitalAPI.Controllers.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ValidToken")]
    public class ConfigurationController : ControllerBase
    {
        IGeneralConfigService _generalConfigService;
        public ConfigurationController(IGeneralConfigService generalConfigService)
        {

            _generalConfigService = generalConfigService;
        }
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddProject(IFormFile file, string name, string path)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _generalConfigService.UploadFiles(file, name, path));
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf(string path)
        {

            var fullpath = Path.Combine(Directory.GetCurrentDirectory(), path);

            var bytes = System.IO.File.ReadAllBytes(fullpath);
            return File(bytes, "application/pdf");

        }
    }
}
