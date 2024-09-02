using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;

namespace SMSServiceImplementation.DTOS.Message
{
    public class MessageGetDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string MessageStatus { get; set; }
        public string Language { get; set; }
        public string MessageGroup { get; set; }
        public Guid MessageGroupId { get; set; }
        public bool IsApproved { get; set; }
        public int TextSize { get; set; }
        public int NumberOfCustomer { get; set; }
        public DateTime CreatedDate { get; set; }

        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }



    }

    public class MessagePostDto
    {
        public string Content { get; set; }       
        public string Language { get; set; }
        public Guid MessageGroupId { get; set; }       
        public string CreatedById { get; set; }
        public Guid OrganizationId { get; set; }
    }
}
