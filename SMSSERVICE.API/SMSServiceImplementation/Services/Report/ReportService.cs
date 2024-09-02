using AutoMapper;
using AutoMapper.QueryableExtensions;
using IntegratedImplementation.DTOS.HRM;
using IntegratedInfrustructure.Data;
using IntegratedInfrustructure.Model.HRM;
using Microsoft.EntityFrameworkCore;
using SMSServiceImplementation.DTOS.Message;
using SMSServiceImplementation.DTOS.Report;
using SMSServiceImplementation.Interfaces.Report;
using SMSServiceInfrustructure.Model.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;
using static IntegratedInfrustructure.Data.EnumList;

namespace SMSServiceImplementation.Services.Report
{
    public class ReportService : IReport
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public ReportService(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            //_mapper = mapper;
        }

        public async Task<List<GetReportDto>> GetReports(Guid messageGroupId)
        {
            var messages = await _dbContext.Messages
                .Include(x => x.MessageGroup)
                .Include(x => x.MessageGroup.Organization)
                .Include(x => x.MessageGroup.GroupPhoneNumbers)
                //.Include(x => x.MessageGroup.OrganizationId)
                .Where(x => x.MessageGroupId == messageGroupId)
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
                    OrganizationName = x.MessageGroup.Organization.Name,
                    CreatedDate = x.CreatedDate

                }).ToListAsync();
            var reports = new List<GetReportDto>();
            foreach (var message in messages)
            {
                var reportq = GenerateReportFromMessage(message);
                reports.Add(reportq);
            }
            return reports;
            
        }
        public GetReportDto GenerateReportFromMessage(MessageGetDto message)
        {
            var reportw = new GetReportDto
            {
                OrganizationId = message.OrganizationId,
                MessageGroupId = message.MessageGroupId,
                Content = message.Content,
                GroupName = message.MessageGroup,
                NumberOfCustomer = message.NumberOfCustomer,
                OrganizationName = message.OrganizationName,
                MessageStatus = message.MessageStatus,
                Language = message.Language,
                CreatedDate = message.CreatedDate
            };

            return reportw;
        }
    }
}
