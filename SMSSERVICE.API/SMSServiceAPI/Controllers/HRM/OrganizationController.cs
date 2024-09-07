using Implementation.Helper;
using IntegratedImplementation.DTOS.Configuration;
using IntegratedImplementation.DTOS.HRM;
using IntegratedImplementation.Interfaces.HRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IntegratedDigitalAPI.Controllers.HRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ValidToken")]
    public class OrganizationController : ControllerBase
    {
        IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(OrganizationGetDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Getorganizations()
        {
            return Ok(await _organizationService.Getorganizations());
        }
        [HttpGet("getorganization")]
        [ProducesResponseType(typeof(OrganizationGetDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Getorganization(Guid organizationId)
        {
            return Ok(await _organizationService.Getorganization(organizationId));
        }
        [HttpGet("getorganizationNoUser")]
        [ProducesResponseType(typeof(OrganizationGetDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetorganizationNoUser()
        {
            return Ok(await _organizationService.GetorganizationNoUser());
        }


        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Addorganization([FromForm] OrganizationPostDto organization)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _organizationService.Addorganization(organization));
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPut]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Updateorganization([FromForm] OrganizationGetDto organization)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _organizationService.Updateorganization(organization));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("getorganizationsSelectList")]
        [ProducesResponseType(typeof(SelectListDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetorganizationsSelectList()
        {
            return Ok(await _organizationService.GetorganizationSelectList());
        }
        [HttpPut("changeorganizationImage")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> changeorganizationImage([FromForm]OrganizationPostDto addorganization)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _organizationService.changeorganizationImage(addorganization));
            }
            else
            {
                return BadRequest();
            }
        }




    }
}
