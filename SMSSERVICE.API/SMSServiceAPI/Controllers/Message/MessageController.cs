using Implementation.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMSServiceImplementation.DTOS.Message;
using SMSServiceImplementation.Interfaces.Message;
using System.Net;
using Microsoft.Extensions.Logging;

namespace SMSServiceAPI.Controllers.Message
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = "ValidToken")]
    public class MessageController : ControllerBase
    {

        IMessageService _MessageService;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IMessageService messageService, ILogger<MessageController> logger)
        {
             _MessageService = messageService;
             _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(MessageGetDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessages(Guid MessageGroupId)
        {
            // Validate GUID input
            if (!InputValidationService.ValidateGuid(MessageGroupId.ToString()))
            {
                return BadRequest(new ResponseMessage { Success = false, Message = "Invalid MessageGroupId format" });
            }

            return Ok(await _MessageService.GetMessages(MessageGroupId));
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(MessageGetDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUnsentMessages(Guid? organizationId)
        {
            // Validate GUID input if provided
            if (organizationId.HasValue && !InputValidationService.ValidateGuid(organizationId.Value.ToString()))
            {
                return BadRequest(new ResponseMessage { Success = false, Message = "Invalid organizationId format" });
            }

            return Ok(await _MessageService.GetUnsentMessages(organizationId));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddMessages(MessagePostDto messagePost)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Server-side validation using InputValidationService
                    var (isValid, errors) = InputValidationService.ValidateMessageData(
                        messagePost.Content
                    );

                    if (!isValid)
                    {
                        return BadRequest(ErrorHandlingService.HandleValidationErrors(errors));
                    }

                    var result = await _MessageService.AddMessages(messagePost);
                    return Ok(ErrorHandlingService.CreateSuccessResponse("Message created successfully.", result.Data));
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
                return StatusCode(500, ErrorHandlingService.HandleException(ex, _logger, "AddMessages"));
            }
        }

        /// <summary>
        /// Reject a message with a reason
        /// </summary>
        [HttpPost("reject")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RejectMessage([FromBody] MessageRejectDto rejectDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validate GUID input
                    if (!InputValidationService.ValidateGuid(rejectDto.MessageId.ToString()))
                    {
                        return BadRequest(ErrorHandlingService.CreateErrorResponse(
                            "InvalidFormat", "Invalid message ID format."));
                    }

                    // Validate rejection reason
                    if (string.IsNullOrWhiteSpace(rejectDto.RejectionReason))
                    {
                        return BadRequest(ErrorHandlingService.CreateErrorResponse(
                            "RequiredField", "Please provide a reason for rejecting this message."));
                    }

                    if (rejectDto.RejectionReason.Length < 10)
                    {
                        return BadRequest(ErrorHandlingService.CreateErrorResponse(
                            "TooShort", "Rejection reason must be at least 10 characters long."));
                    }

                    if (rejectDto.RejectionReason.Length > 500)
                    {
                        return BadRequest(ErrorHandlingService.CreateErrorResponse(
                            "TooLong", "Rejection reason cannot exceed 500 characters."));
                    }

                    // Sanitize rejection reason
                    var sanitizedReason = InputValidationService.SanitizeHtml(rejectDto.RejectionReason);
                    rejectDto.RejectionReason = sanitizedReason;

                    var result = await _MessageService.RejectMessage(rejectDto);
                    return Ok(ErrorHandlingService.CreateSuccessResponse("Message rejected successfully.", result.Data));
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
                return StatusCode(500, ErrorHandlingService.HandleException(ex, _logger, "RejectMessage"));
            }
        }

        /// <summary>
        /// Approve a message
        /// </summary>
        [HttpPost("approve")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ApproveMessage([FromBody] MessageApproveDto approveDto)
        {
            if (ModelState.IsValid)
            {
                // Validate GUID input
                if (!InputValidationService.ValidateGuid(approveDto.MessageId.ToString()))
                {
                    return BadRequest(new ResponseMessage { Success = false, Message = "Invalid MessageId format" });
                }

                // Validate approval notes if provided
                if (!string.IsNullOrWhiteSpace(approveDto.ApprovalNotes))
                {
                    if (approveDto.ApprovalNotes.Length > 200)
                    {
                        return BadRequest(new ResponseMessage { Success = false, Message = "Approval notes cannot exceed 200 characters" });
                    }

                    // Sanitize approval notes
                    approveDto.ApprovalNotes = InputValidationService.SanitizeHtml(approveDto.ApprovalNotes);
                }

                return Ok(await _MessageService.ApproveMessage(approveDto));
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new ResponseMessage 
                { 
                    Success = false, 
                    Message = "Validation failed", 
                    Data = errors 
                });
            }
        }

        /// <summary>
        /// Update message status
        /// </summary>
        [HttpPut("update-status")]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateMessageStatus([FromBody] MessageStatusUpdateDto statusUpdate)
        {
            if (ModelState.IsValid)
            {
                // Validate GUID input
                if (!InputValidationService.ValidateGuid(statusUpdate.MessageId.ToString()))
                {
                    return BadRequest(new ResponseMessage { Success = false, Message = "Invalid MessageId format" });
                }

                // Validate status
                var allowedStatuses = new[] { "Pending", "Approved", "Rejected", "Sent", "Failed", "Delivered" };
                if (!allowedStatuses.Contains(statusUpdate.Status))
                {
                    return BadRequest(new ResponseMessage 
                    { 
                        Success = false, 
                        Message = $"Invalid status. Allowed statuses: {string.Join(", ", allowedStatuses)}" 
                    });
                }

                // Validate status notes if provided
                if (!string.IsNullOrWhiteSpace(statusUpdate.StatusNotes))
                {
                    if (statusUpdate.StatusNotes.Length > 500)
                    {
                        return BadRequest(new ResponseMessage { Success = false, Message = "Status notes cannot exceed 500 characters" });
                    }

                    // Sanitize status notes
                    statusUpdate.StatusNotes = InputValidationService.SanitizeHtml(statusUpdate.StatusNotes);
                }

                return Ok(await _MessageService.UpdateMessageStatus(statusUpdate));
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new ResponseMessage 
                { 
                    Success = false, 
                    Message = "Validation failed", 
                    Data = errors 
                });
            }
        }

        /// <summary>
        /// Get message details including rejection/approval information
        /// </summary>
        [HttpGet("details/{messageId}")]
        [ProducesResponseType(typeof(MessageDetailDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessageDetails(Guid messageId)
        {
            // Validate GUID input
            if (!InputValidationService.ValidateGuid(messageId.ToString()))
            {
                return BadRequest(new ResponseMessage { Success = false, Message = "Invalid MessageId format" });
            }

            return Ok(await _MessageService.GetMessageDetails(messageId));
        }

        /// <summary>
        /// Get rejected messages
        /// </summary>
        [HttpGet("rejected")]
        [ProducesResponseType(typeof(MessageDetailDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRejectedMessages(Guid? organizationId)
        {
            // Validate GUID input if provided
            if (organizationId.HasValue && !InputValidationService.ValidateGuid(organizationId.Value.ToString()))
            {
                return BadRequest(new ResponseMessage { Success = false, Message = "Invalid organizationId format" });
            }

            return Ok(await _MessageService.GetRejectedMessages(organizationId));
        }

        /// <summary>
        /// Get pending messages for approval
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(MessageDetailDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPendingMessages(Guid? organizationId)
        {
            // Validate GUID input if provided
            if (organizationId.HasValue && !InputValidationService.ValidateGuid(organizationId.Value.ToString()))
            {
                return BadRequest(new ResponseMessage { Success = false, Message = "Invalid organizationId format" });
            }

            return Ok(await _MessageService.GetPendingMessages(organizationId));
        }
    }
}
