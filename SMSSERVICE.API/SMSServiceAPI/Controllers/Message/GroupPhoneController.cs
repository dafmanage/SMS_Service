using Implementation.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMSServiceImplementation.DTOS.Message;
using SMSServiceImplementation.Interfaces.Message;
using System.Net;

namespace SMSServiceAPI.Controllers.Message
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = "ValidToken")]
    public class GroupPhoneController : ControllerBase
    {
        IGroupPhoneService _GroupPhoneService;

        public GroupPhoneController(IGroupPhoneService GroupPhoneService)
        {
            _GroupPhoneService = GroupPhoneService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GroupPhoneGetDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetGroupPhones(Guid MessageGroupId)
        {
            return Ok(await _GroupPhoneService.GetGroupPhones(MessageGroupId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddGroupPhone( GroupPhonePostDto GroupPhone)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _GroupPhoneService.AddGroupPhone(GroupPhone));
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddGroupPhoneFromExcel(IFormFile ExcelFile, string createdById)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _GroupPhoneService.AddGroupPhoneFromExcel(ExcelFile,createdById));
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPut]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateGroupPhone(GroupPhoneGetDto GroupPhone)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _GroupPhoneService.UpdateGroupPhone(GroupPhone));
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
