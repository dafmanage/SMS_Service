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

namespace SMSServiceImplementation.Services.Message
{
    public class MessageGroupService : IMessageGroupService
    {

        private readonly ApplicationDbContext _dbContext;

        private readonly IMapper _mapper;
        public MessageGroupService(ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _dbContext = dbContext;


            _mapper = mapper;
        }

        public async Task<ResponseMessage> AddMessageGroup(MessageGroupPostDto addMessageGroup)
        {


            MessageGroup messageGroup = new MessageGroup
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                CreatedById = addMessageGroup.CreatedById,
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

                Message = "Added Successfully",
                Success = true
            };
        }

        public async Task<ResponseMessage> UpdateMessageGroup(MessageGroupGetDto addMessageGroup)
        {

           




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

        public async Task<List<MessageGroupGetDto>> GetMessageGroups(Guid OrganizationId)
        {
            var MessageGroupList = await _dbContext.MessageGroups.Where(x=>x.OrganizationId == OrganizationId).AsNoTracking()
                                    .ProjectTo<MessageGroupGetDto>(_mapper.ConfigurationProvider)
                                    .ToListAsync();
            return MessageGroupList;
        }
    }
}
