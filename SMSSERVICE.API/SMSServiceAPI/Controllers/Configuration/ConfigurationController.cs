using Implementation.Helper;
using IntegratedImplementation.Interfaces.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Logging;

namespace IntegratedDigitalAPI.Controllers.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ValidToken")]
    public class ConfigurationController : ControllerBase
    {
        IGeneralConfigService _generalConfigService;
        private readonly ILogger<ConfigurationController> _logger;
        
        public ConfigurationController(IGeneralConfigService generalConfigService, ILogger<ConfigurationController> logger)
        {
            _generalConfigService = generalConfigService;
            _logger = logger;
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddProject(IFormFile file, string name, string path)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validate file upload - only images and Excel files allowed
                    var (isFileValid, fileError) = InputValidationService.ValidateImageAndExcelUpload(file);
                    
                    if (!isFileValid)
                    {
                        return BadRequest(ErrorHandlingService.CreateErrorResponse(
                            "InvalidFileType", fileError));
                    }

                    // Validate name and path inputs
                    var (isNameValid, _) = InputValidationService.ValidateAlphanumeric(name);
                    var (isPathValid, _) = InputValidationService.ValidateAlphanumeric(path);
                    
                    if (!isNameValid)
                    {
                        return BadRequest(ErrorHandlingService.CreateErrorResponse(
                            "InvalidFormat", "Please enter a valid project name."));
                    }
                    
                    if (!isPathValid)
                    {
                        return BadRequest(ErrorHandlingService.CreateErrorResponse(
                            "InvalidFormat", "Please enter a valid path."));
                    }

                    var result = await _generalConfigService.UploadFiles(file, name, path);
                    return Ok(ErrorHandlingService.CreateSuccessResponse("File uploaded successfully.", result));
                }
                else
                {
                    var validationErrors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList();
                    
                    return BadRequest(ErrorHandlingService.HandleValidationErrors(validationErrors));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ErrorHandlingService.HandleFileException(ex, _logger));
            }
        }
        
        [HttpGet("pdf")]
        public async Task<IActionResult> GetPdf(string path)
        {
            // Validate path input
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest(new ResponseMessage { Success = false, Message = "Path cannot be empty" });
            }

            // Additional path validation to prevent directory traversal attacks
            if (path.Contains("..") || path.Contains("\\") || path.StartsWith("/"))
            {
                return BadRequest(new ResponseMessage { Success = false, Message = "Invalid path format" });
            }

            var fullpath = Path.Combine(Directory.GetCurrentDirectory(), path);

            // Verify file exists and is accessible
            if (!System.IO.File.Exists(fullpath))
            {
                return NotFound(new ResponseMessage { Success = false, Message = "File not found" });
            }

            var bytes = System.IO.File.ReadAllBytes(fullpath);
            return File(bytes, "application/pdf");
        }
    }
}
