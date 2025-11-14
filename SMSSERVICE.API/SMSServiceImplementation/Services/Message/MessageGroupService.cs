using AutoMapper;
using AutoMapper.QueryableExtensions;
using Implementation.Helper;
using IntegratedImplementation.DTOS.HRM;
using IntegratedImplementation.Interfaces.Configuration;
using IntegratedInfrustructure.Data;
using IntegratedInfrustructure.Model.Authentication;
using IntegratedInfrustructure.Model.HRM;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMSServiceImplementation.DTOS.Message;
using SMSServiceImplementation.Interfaces.Message;
using SMSServiceInfrustructure.Model.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;
using Microsoft.AspNetCore.Http;

namespace SMSServiceImplementation.Services.Message
{
    public class MessageGroupService : IMessageGroupService
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        
        public MessageGroupService(ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<ResponseMessage> AddMessageGroup(MessageGroupPostDto addMessageGroup)
        {
            try
            {
                // Get current user
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                if (currentUser == null)
                {
                    // Try to get user from claims if UserManager fails
                    var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                        ?? _httpContextAccessor.HttpContext.User.FindFirst("userId");
                    if (userIdClaim != null)
                    {
                        currentUser = await _userManager.FindByIdAsync(userIdClaim.Value);
                    }
                    
                    if (currentUser == null)
                        return new ResponseMessage { Success = false, Message = "User not authenticated" };
                }

                // Check if user is Admin (only Admin can add message groups)
                var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
                var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");

                if (!isAdmin)
                    return new ResponseMessage { Success = false, Message = "Only Admin users can create message groups. SuperAdmin users have read-only access." };

                // Validate organization access
                if (!isSuperAdmin)
                {
                    // Non-SuperAdmin users can only create groups for their own organization
                    if (addMessageGroup.OrganizationId != currentUser.OrganizationId)
                        return new ResponseMessage { Success = false, Message = "You can only create message groups for your own organization" };
                }

                MessageGroup messageGroup = new MessageGroup
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    CreatedById = currentUser.Id, // Use current user's ID
                    GroupCode = addMessageGroup.GroupCode,
                    GroupName = addMessageGroup.GroupName,
                    Remark = addMessageGroup.Remark,
                    OrganizationId = addMessageGroup.OrganizationId,
                    Rowstatus = RowStatus.ACTIVE,
                };
                
                await _dbContext.MessageGroups.AddAsync(messageGroup);
                await _dbContext.SaveChangesAsync();

                return new ResponseMessage
                {
                    Message = "Message group created successfully",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessage { Success = false, Message = $"Error creating message group: {ex.Message}" };
            }
        }

        public async Task<ResponseMessage> UpdateMessageGroup(MessageGroupGetDto addMessageGroup)
        {
            try
            {
                // Get current user
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                if (currentUser == null)
                {
                    // Try to get user from claims if UserManager fails
                    var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                        ?? _httpContextAccessor.HttpContext.User.FindFirst("userId");
                    if (userIdClaim != null)
                    {
                        currentUser = await _userManager.FindByIdAsync(userIdClaim.Value);
                    }
                    
                    if (currentUser == null)
                        return new ResponseMessage { Success = false, Message = "User not authenticated" };
                }

                // Check if user is Admin (only Admin can update message groups)
                var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

                if (!isAdmin)
                    return new ResponseMessage { Success = false, Message = "Only Admin users can update message groups. SuperAdmin users have read-only access." };

                var MessageGroup = _dbContext.MessageGroups.Find(addMessageGroup.Id);

            if (MessageGroup != null)
            {

                MessageGroup.GroupCode = addMessageGroup.GroupCode;      
                MessageGroup.GroupName = addMessageGroup.GroupName;
                MessageGroup.Remark = addMessageGroup.Remark;
     


                await _dbContext.SaveChangesAsync();

                return new ResponseMessage
                {

                    Message = "Updated Successfully",
                    Success = true
                };

            }
            else
            {
                return new ResponseMessage
                {
                    Message = "No MessageGroup Found",
                    Success = false
                };
            }
            }
            catch (Exception ex)
            {
                return new ResponseMessage { Success = false, Message = $"Error updating message group: {ex.Message}" };
            }
        }

        public async Task<List<MessageGroupGetDto>> GetMessageGroups(Guid OrganizationId)
        {
            Console.WriteLine($"DEBUG: GetMessageGroups called with OrganizationId: {OrganizationId}");
            
            // Get current user
            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (currentUser == null)
            {
                // Try to get user from claims if UserManager fails
                var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    currentUser = await _userManager.FindByIdAsync(userIdClaim.Value);
                }
            }
            
            Console.WriteLine($"DEBUG: Current user: {currentUser?.UserName ?? "null"}");
            Console.WriteLine($"DEBUG: Current user organization: {currentUser?.OrganizationId}");
            
            if (currentUser == null) 
            {
                Console.WriteLine("DEBUG: No current user found, returning empty list");
                return new List<MessageGroupGetDto>();
            }

            // Check if user is SuperAdmin
            var isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            
            Console.WriteLine($"DEBUG: User roles - SuperAdmin: {isSuperAdmin}, Admin: {isAdmin}");
            
            if (isSuperAdmin)
            {
                // SuperAdmin can see all message groups for the requested organization
                Console.WriteLine($"DEBUG: SuperAdmin querying for OrganizationId: {OrganizationId}");
                var MessageGroupList = await _dbContext.MessageGroups
                    .Where(x => x.OrganizationId == OrganizationId)
                    .AsNoTracking()
                    .ProjectTo<MessageGroupGetDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
                Console.WriteLine($"DEBUG: SuperAdmin found {MessageGroupList.Count} message groups");
                return MessageGroupList;
            }
            else if (isAdmin)
            {
                // Admin users can only see message groups from their own organization
                // Ignore the OrganizationId parameter and use the user's organization
                Console.WriteLine($"DEBUG: Admin user - ignoring OrganizationId parameter: {OrganizationId}");
                Console.WriteLine($"DEBUG: Admin querying for user's OrganizationId: {currentUser.OrganizationId}");
                
                // Verify that the requested organization matches the user's organization
                if (OrganizationId != currentUser.OrganizationId)
                {
                    Console.WriteLine($"DEBUG: Admin user trying to access different organization. Requested: {OrganizationId}, User's: {currentUser.OrganizationId}");
                    return new List<MessageGroupGetDto>();
                }
                
                var MessageGroupList = await _dbContext.MessageGroups
                    .Where(x => x.OrganizationId == currentUser.OrganizationId)
                    .AsNoTracking()
                    .ProjectTo<MessageGroupGetDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
                Console.WriteLine($"DEBUG: Admin found {MessageGroupList.Count} message groups");
                return MessageGroupList;
            }
            else
            {
                // Other users have no access
                Console.WriteLine("DEBUG: User has no Admin or SuperAdmin role, returning empty list");
                return new List<MessageGroupGetDto>();
            }
        }

        public async Task<ResponseMessage> CreateSampleMessageGroups()
        {
            try
            {
                // Get current user
                var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                if (currentUser == null)
                {
                    // Try to get user from claims if UserManager fails
                    var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                        ?? _httpContextAccessor.HttpContext.User.FindFirst("userId");
                    
                    if (userIdClaim != null)
                    {
                        currentUser = await _userManager.FindByIdAsync(userIdClaim.Value);
                    }
                    
                    if (currentUser == null)
                        return new ResponseMessage { Success = false, Message = "User not authenticated" };
                }

                // Check if user is Admin (only Admin can create message groups)
                var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

                if (!isAdmin)
                    return new ResponseMessage { Success = false, Message = "Only Admin users can create sample message groups." };

                // Check if sample groups already exist
                var existingGroups = await _dbContext.MessageGroups
                    .Where(x => x.OrganizationId == currentUser.OrganizationId)
                    .CountAsync();

                if (existingGroups > 0)
                {
                    return new ResponseMessage { Success = true, Message = $"Sample message groups already exist. Found {existingGroups} groups." };
                }

                // Create sample message groups
                var sampleGroups = new[]
                {
                    new MessageGroup
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = currentUser.OrganizationId,
                        GroupName = "Marketing Team",
                        GroupCode = "MKT001",
                        Remark = "Marketing department message group",
                        CreatedDate = DateTime.UtcNow,
                        CreatedById = currentUser.Id,
                        Rowstatus = RowStatus.ACTIVE
                    },
                    new MessageGroup
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = currentUser.OrganizationId,
                        GroupName = "Sales Team",
                        GroupCode = "SALES001",
                        Remark = "Sales department message group",
                        CreatedDate = DateTime.UtcNow,
                        CreatedById = currentUser.Id,
                        Rowstatus = RowStatus.ACTIVE
                    },
                    new MessageGroup
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = currentUser.OrganizationId,
                        GroupName = "Support Team",
                        GroupCode = "SUP001",
                        Remark = "Customer support team group",
                        CreatedDate = DateTime.UtcNow,
                        CreatedById = currentUser.Id,
                        Rowstatus = RowStatus.ACTIVE
                    },
                    new MessageGroup
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = currentUser.OrganizationId,
                        GroupName = "Management",
                        GroupCode = "MGMT001",
                        Remark = "Management team group",
                        CreatedDate = DateTime.UtcNow,
                        CreatedById = currentUser.Id,
                        Rowstatus = RowStatus.ACTIVE
                    },
                    new MessageGroup
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = currentUser.OrganizationId,
                        GroupName = "IT Department",
                        GroupCode = "IT001",
                        Remark = "IT department group",
                        CreatedDate = DateTime.UtcNow,
                        CreatedById = currentUser.Id,
                        Rowstatus = RowStatus.ACTIVE
                    }
                };

                _dbContext.MessageGroups.AddRange(sampleGroups);
                await _dbContext.SaveChangesAsync();

                return new ResponseMessage
                {
                    Success = true,
                    Message = $"Successfully created {sampleGroups.Length} sample message groups for organization {currentUser.OrganizationId}",
                    Data = new { CreatedCount = sampleGroups.Length, OrganizationId = currentUser.OrganizationId }
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessage { Success = false, Message = $"Error creating sample message groups: {ex.Message}" };
            }
        }
    }
}
