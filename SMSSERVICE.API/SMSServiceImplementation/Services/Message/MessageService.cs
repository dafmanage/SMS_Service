using AutoMapper;
using Implementation.Helper;
using IntegratedInfrustructure.Data;
using IntegratedInfrustructure.Model.Authentication;
using IntegratedInfrustructure.Model.HRM;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMSServiceImplementation.DTOS.Message;
using SMSServiceImplementation.Interfaces.Message;
using SMSServiceInfrustructure.Migrations;
using SMSServiceInfrustructure.Model.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;
using Microsoft.AspNetCore.Http;

namespace SMSServiceImplementation.Services.Message
{
    public class MessageService :IMessageService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        
        public MessageService(ApplicationDbContext dbContext, 
            IMapper mapper, 
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<MessageGetDto>> GetMessages(Guid messageGroupId)
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (currentUser == null) return new List<MessageGetDto>();

            // Check if user is SuperAdmin
            var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
            
            var query = _dbContext.Messages
                .Include(x => x.MessageGroup)
                .Include(x => x.MessageGroup.GroupPhoneNumbers)
                .Where(x => x.MessageGroupId == messageGroupId);

            // Apply organization filtering for non-SuperAdmin users
            if (!isSuperAdmin)
            {
                query = query.Where(x => x.OrganizationId == currentUser.OrganizationId);
            }

            var results = await query
                .Select(x => new MessageGetDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    MessageGroupId = x.MessageGroupId,
                    OrganizationId = x.OrganizationId,
                    MessageGroup = x.MessageGroup.GroupName,
                    MessageStatus = x.MessageStatus.ToString(),
                    Language = x.Language.ToString(),
                    IsApproved = x.IsApproved,
                    TextSize = x.TextSize,
                    NumberOfCustomer = x.MessageGroup.GroupPhoneNumbers.Count(),
                    CreatedDate = x.CreatedDate
                }).ToListAsync();

            return results;
        }
        public async Task<ResponseMessage> AddMessages(MessagePostDto addMessages)
        {
            try
            {
                // Get current user
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                if (currentUser == null)
                    return new ResponseMessage { Success = false, Message = "User not authenticated" };

                // Check if user is SuperAdmin or Admin
                var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
                var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

                if (!isSuperAdmin && !isAdmin)
                    return new ResponseMessage { Success = false, Message = "Insufficient permissions to create messages" };

                // Validate organization access
                if (!isSuperAdmin)
                {
                    // Non-SuperAdmin users can only create messages for their own organization
                    if (addMessages.OrganizationId != currentUser.OrganizationId)
                        return new ResponseMessage { Success = false, Message = "You can only create messages for your own organization" };
                }

                // Determine initial status based on user role
                MessageStatus initialStatus;
                bool isApproved;
                
                if (isSuperAdmin)
                {
                    // SuperAdmin messages are automatically approved
                    initialStatus = MessageStatus.Pending;
                    isApproved = true;
                }
                else
                {
                    // Regular users' messages need approval
                    initialStatus = MessageStatus.Pending;
                    isApproved = false;
                }

                SMSServiceInfrustructure.Model.Message.Message messagePost = new SMSServiceInfrustructure.Model.Message.Message
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    CreatedById = currentUser.Id, // Use current user's ID
                    Content = addMessages.Content,
                    Language = Enum.Parse<MessageLanguage>(addMessages.Language),
                    MessageStatus = initialStatus,
                    IsApproved = isApproved,
                    MessageGroupId = addMessages.MessageGroupId,
                    OrganizationId = addMessages.OrganizationId,
                    Rowstatus = RowStatus.ACTIVE,
                };
                
                messagePost.TextSize = GetTextSize(messagePost.Content, messagePost.Language);
                messagePost.NumberOfCustomer = _dbContext.MessageGroupPhones.Where(x => x.MessageGroupId == addMessages.MessageGroupId).Count();

                await _dbContext.Messages.AddAsync(messagePost);
                await _dbContext.SaveChangesAsync();

            var messagePhoneNumbers = await _dbContext.MessageGroupPhones.Where(x=>x.MessageGroupId==addMessages.MessageGroupId).ToListAsync();

            foreach (var phoneNumber in messagePhoneNumbers)
            {
                PersonalMessages personalMessage = new PersonalMessages
                {
                    MessageId = messagePost.Id,
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    CreatedById = addMessages.CreatedById,
                    PhoneNumber = phoneNumber.PhoneNumber,

                };

                personalMessage.MessageStatus = MessageStatus.UNSENT;

                await _dbContext.PersonalMessages.AddAsync(personalMessage);
                await _dbContext.SaveChangesAsync();
            }    

            return new ResponseMessage
            {

                Message = "Added Successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new ResponseMessage { Success = false, Message = $"Error creating message: {ex.Message}" };
        }
    }

        public async Task<List<MessageGetDto>> GetUnsentMessages(Guid? organizationId)
        {
            var results = await _dbContext.Messages
             .Include(x => x.MessageGroup.Organization)         
             .Where(x => !x.IsApproved&&x.MessageStatus==MessageStatus.UNSENT)
             .Select(x => new MessageGetDto
             {
                 Id = x.Id,
                 Content = x.Content,
                 MessageGroup = x.MessageGroup.GroupName,
                 MessageStatus = x.MessageStatus.ToString(),
                 Language = x.Language.ToString(),
                 IsApproved = x.IsApproved,
                 
                 NumberOfCustomer = x.NumberOfCustomer,
                 MessageGroupId = x.MessageGroupId,
                 OrganizationId=x.MessageGroup.OrganizationId,
                 OrganizationName =x.MessageGroup.Organization.Name,
                 //TextSize = GetTextSize(x.Content, x.Language),
                 TextSize = x.TextSize,

             }).ToListAsync();

            if(organizationId != null)
            {
                results = results.Where(x=>x.OrganizationId == organizationId).ToList();
            }
            foreach (var message in results)
            {
                message.TextSize = GetTextSize(message.Content, (MessageLanguage)Enum.Parse(typeof(MessageLanguage), message.Language));
            }
            return results;
        }
    

        public int GetTextSize(string content, MessageLanguage messageLanguage)
        {
            int contentTextSize = content.Length;
            int[] thresholds = GetMessageLanguageThresholds(messageLanguage);
            int textSize = 1;



            foreach (int threshold in thresholds)
            {
                if (contentTextSize > threshold)
                {
                    textSize++;
                }
                else
                {
                    break;
                }
            }

            return textSize;
        }

        private int[] GetMessageLanguageThresholds(MessageLanguage messageLanguage)
        {
            if (messageLanguage == MessageLanguage.ENGLISH)
            {
                return new int[] { 100, 200, 300,400,500 };
            }
            else
            {
                return new int[] { 58, 116, 174,232,290 };
            }
        }

        public async Task<ResponseMessage> RejectMessage(MessageRejectDto rejectDto)
        {
            try
            {
                var message = await _dbContext.Messages.FindAsync(rejectDto.MessageId);
                if (message == null)
                {
                    return new ResponseMessage
                    {
                        Success = false,
                        Message = "Message not found.",
                        ErrorCode = 404
                    };
                }

                message.MessageStatus = MessageStatus.Rejected;
                message.IsApproved = false;
                
                await _dbContext.SaveChangesAsync();

                return new ResponseMessage
                {
                    Success = true,
                    Message = "Message rejected successfully.",
                    Data = new { MessageId = message.Id, Status = "Rejected" }
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "An error occurred while rejecting the message.",
                    ErrorCode = 500
                };
            }
        }

        public async Task<ResponseMessage> ApproveMessage(MessageApproveDto approveDto)
        {
            try
            {
                // Get current user
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                if (currentUser == null)
                    return new ResponseMessage { Success = false, Message = "User not authenticated" };

                // Only SuperAdmin can approve messages
                var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
                if (!isSuperAdmin)
                    return new ResponseMessage { Success = false, Message = "Only SuperAdmin can approve messages" };

                var message = await _dbContext.Messages.FindAsync(approveDto.MessageId);
                if (message == null)
                {
                    return new ResponseMessage
                    {
                        Success = false,
                        Message = "Message not found.",
                        ErrorCode = 404
                    };
                }

                message.MessageStatus = MessageStatus.Approved;
                message.IsApproved = true;
                
                await _dbContext.SaveChangesAsync();

                return new ResponseMessage
                {
                    Success = true,
                    Message = "Message approved successfully.",
                    Data = new { MessageId = message.Id, Status = "Approved" }
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "An error occurred while approving the message.",
                    ErrorCode = 500
                };
            }
        }

        public async Task<ResponseMessage> UpdateMessageStatus(MessageStatusUpdateDto statusUpdate)
        {
            try
            {
                var message = await _dbContext.Messages.FindAsync(statusUpdate.MessageId);
                if (message == null)
                {
                    return new ResponseMessage
                    {
                        Success = false,
                        Message = "Message not found.",
                        ErrorCode = 404
                    };
                }

                if (Enum.TryParse<MessageStatus>(statusUpdate.Status, out var newStatus))
                {
                    message.MessageStatus = newStatus;
                    message.IsApproved = newStatus == MessageStatus.Approved;
                    
                    await _dbContext.SaveChangesAsync();

                    return new ResponseMessage
                    {
                        Success = true,
                        Message = "Message status updated successfully.",
                        Data = new { MessageId = message.Id, Status = statusUpdate.Status }
                    };
                }
                else
                {
                    return new ResponseMessage
                    {
                        Success = false,
                        Message = "Invalid status value.",
                        ErrorCode = 400
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseMessage
                {
                    Success = false,
                    Message = "An error occurred while updating the message status.",
                    ErrorCode = 500
                };
            }
        }

        public async Task<MessageDetailDto> GetMessageDetails(Guid messageId)
        {
            var message = await _dbContext.Messages
                .Include(x => x.MessageGroup)
                .Include(x => x.MessageGroup.GroupPhoneNumbers)
                .Include(x => x.MessageGroup.Organization)
                .FirstOrDefaultAsync(x => x.Id == messageId);

            if (message == null)
            {
                return new MessageDetailDto();
            }

            return new MessageDetailDto
            {
                Id = message.Id,
                Content = message.Content,
                MessageGroupId = message.MessageGroupId,
                OrganizationId = message.OrganizationId,
                MessageGroup = message.MessageGroup.GroupName,
                MessageStatus = message.MessageStatus.ToString(),
                Language = message.Language.ToString(),
                IsApproved = message.IsApproved,
                TextSize = message.TextSize,
                NumberOfCustomer = message.MessageGroup.GroupPhoneNumbers.Count(),
                CreatedDate = message.CreatedDate,
                OrganizationName = message.MessageGroup.Organization.Name,
                RejectionReason = null,
                RejectedAt = null,
                RejectedBy = null,
                ApprovedAt = message.IsApproved ? message.CreatedDate : null,
                ApprovedBy = null,
                ApprovalNotes = null,
                StatusNotes = null,
                LastStatusUpdate = message.CreatedDate,
                LastUpdatedBy = null
            };
        }

        public async Task<List<MessageDetailDto>> GetRejectedMessages(Guid? organizationId)
        {
            var results = await _dbContext.Messages
                .Include(x => x.MessageGroup)
                .Include(x => x.MessageGroup.GroupPhoneNumbers)
                .Include(x => x.MessageGroup.Organization)
                .Where(x => x.MessageStatus == MessageStatus.Rejected && 
                           (organizationId == null || x.OrganizationId == organizationId))
                .Select(x => new MessageDetailDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    MessageGroupId = x.MessageGroupId,
                    OrganizationId = x.OrganizationId,
                    MessageGroup = x.MessageGroup.GroupName,
                    MessageStatus = x.MessageStatus.ToString(),
                    Language = x.Language.ToString(),
                    IsApproved = x.IsApproved,
                    TextSize = x.TextSize,
                    NumberOfCustomer = x.MessageGroup.GroupPhoneNumbers.Count(),
                    CreatedDate = x.CreatedDate,
                    OrganizationName = x.MessageGroup.Organization.Name,
                    RejectionReason = null,
                    RejectedAt = x.CreatedDate,
                    RejectedBy = null,
                    ApprovedAt = null,
                    ApprovedBy = null,
                    ApprovalNotes = null,
                    StatusNotes = null,
                    LastStatusUpdate = x.CreatedDate,
                    LastUpdatedBy = null
                })
                .ToListAsync();

            return results;
        }

        public async Task<List<MessageDetailDto>> GetPendingMessages(Guid? organizationId)
        {
            // Get current user
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (currentUser == null) return new List<MessageDetailDto>();

            // Check if user is SuperAdmin
            var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
            
            var query = _dbContext.Messages
                .Include(x => x.MessageGroup)
                .Include(x => x.MessageGroup.GroupPhoneNumbers)
                .Include(x => x.MessageGroup.Organization)
                .Where(x => x.MessageStatus == MessageStatus.Pending);

            // Apply organization filtering
            if (isSuperAdmin)
            {
                // SuperAdmin can see all pending messages or filter by organization
                if (organizationId.HasValue)
                {
                    query = query.Where(x => x.OrganizationId == organizationId.Value);
                }
            }
            else
            {
                // Regular users can only see pending messages from their organization
                query = query.Where(x => x.OrganizationId == currentUser.OrganizationId);
            }

            var results = await query
                .Select(x => new MessageDetailDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    MessageGroupId = x.MessageGroupId,
                    OrganizationId = x.OrganizationId,
                    MessageGroup = x.MessageGroup.GroupName,
                    MessageStatus = x.MessageStatus.ToString(),
                    Language = x.Language.ToString(),
                    IsApproved = x.IsApproved,
                    TextSize = x.TextSize,
                    NumberOfCustomer = x.MessageGroup.GroupPhoneNumbers.Count(),
                    CreatedDate = x.CreatedDate,
                    OrganizationName = x.MessageGroup.Organization.Name,
                    RejectionReason = null,
                    RejectedAt = null,
                    RejectedBy = null,
                    ApprovedAt = null,
                    ApprovedBy = null,
                    ApprovalNotes = null,
                    StatusNotes = null,
                    LastStatusUpdate = x.CreatedDate,
                    LastUpdatedBy = null
                })
                .ToListAsync();

            return results;
        }
    }
}
