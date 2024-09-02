using Implementation.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMSServiceImplementation.DTOS.Message;
using SMSServiceImplementation.Interfaces.Message;
using System.Net;

namespace SMSServiceAPI.Controllers.Message
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MessageController : ControllerBase
    {

        IMessageService _MessageService;

        public MessageController(IMessageService messageService)
        {
             _MessageService = messageService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(MessageGetDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessages(Guid MessageGroupId)
        {
            return Ok(await _MessageService.GetMessages(MessageGroupId));
        }
        [HttpGet]
        [ProducesResponseType(typeof(MessageGetDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUnsentMessages(Guid? organizationId)
        {
            return Ok(await _MessageService.GetUnsentMessages(organizationId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddMessages(MessagePostDto messagePost)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _MessageService.AddMessages(messagePost));
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
