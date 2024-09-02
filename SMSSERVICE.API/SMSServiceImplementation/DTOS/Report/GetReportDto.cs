using IntegratedInfrustructure.Model.HRM;
using SMSServiceInfrustructure.Model.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;

namespace SMSServiceImplementation.DTOS.Report
{
    public class GetReportDto
    {
        //public Guid ReportId { get; set; }
        public string OrganizationName { get; set; }
        public string Language { get; set; }

        //public MessageGroup MessageGroup { get; set; }
        public string? GroupName { get; set; }
        //public SMSServiceInfrustructure.Model.Message.Message Message { get; set; }
        public string? Content { get; set; }
        public int? NumberOfCustomer { get; set; }
        public string MessageStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UnSentCount { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid MessageGroupId { get; set; }

    }
}
