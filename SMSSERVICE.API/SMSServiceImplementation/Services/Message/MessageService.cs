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

namespace SMSServiceImplementation.Services.Message
{
    public class MessageService :IMessageService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        public MessageService(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<List<MessageGetDto>> GetMessages(Guid messageGroupId)
        {
            var results = await _dbContext.Messages
                .Include(x=>x.MessageGroup)
                .Include(x => x.MessageGroup.GroupPhoneNumbers)
                //.Include(x => x.MessageGroup.OrganizationId)
                .Where(x=>x.MessageGroupId == messageGroupId)                                  
                .Select(x=> new MessageGetDto
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
            //TextSize = GetTextSize(x.Content, x.Language),
            NumberOfCustomer  = x.MessageGroup.GroupPhoneNumbers.Count(),
            CreatedDate = x.CreatedDate


                }).ToListAsync();

            return results;
        }
        public async Task<ResponseMessage> AddMessages(MessagePostDto addMessages)
        {

            SMSServiceInfrustructure.Model.Message.Message messagePost = new SMSServiceInfrustructure.Model.Message.Message
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                CreatedById = addMessages.CreatedById, 
                Content = addMessages.Content,
                Language =Enum.Parse<MessageLanguage>( addMessages.Language),
                MessageStatus = MessageStatus.UNSENT,
                IsApproved = false,
                MessageGroupId = addMessages.MessageGroupId,
                OrganizationId = addMessages.OrganizationId,
                Rowstatus = RowStatus.ACTIVE,
             

            };
            messagePost.TextSize = GetTextSize(messagePost.Content, messagePost.Language);
            messagePost.NumberOfCustomer = _dbContext.MessageGroupPhones.Where(x=>x.MessageGroupId==addMessages.MessageGroupId).Count();

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




    }
}
