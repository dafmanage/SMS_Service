using Implementation.Helper;
using IntegratedImplementation.DTOS.HRM;
using IntegratedImplementation.Interfaces.HRM;
using IntegratedImplementation.Services.HRM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMSServiceImplementation.DTOS.Message;
using SMSServiceImplementation.Interfaces.Message;
using System.Net;

namespace SMSServiceAPI.Controllers.Message
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageGroupController : ControllerBase
    {
        IMessageGroupService _MessageGroupService;

        public MessageGroupController(IMessageGroupService MessageGroupService)
        {
            _MessageGroupService = MessageGroupService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(MessageGroupGetDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessageGroups(Guid OrganizationId )
        {
            return Ok(await _MessageGroupService.GetMessageGroups(OrganizationId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddMessageGroup(MessageGroupPostDto MessageGroup)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _MessageGroupService.AddMessageGroup(MessageGroup));
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPut]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateMessageGroup( MessageGroupGetDto MessageGroup)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _MessageGroupService.UpdateMessageGroup(MessageGroup));
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
