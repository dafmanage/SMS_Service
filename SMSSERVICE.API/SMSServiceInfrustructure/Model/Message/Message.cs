using IntegratedInfrustructure.Model.Authentication;
using IntegratedInfrustructure.Model.HRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;

namespace SMSServiceInfrustructure.Model.Message
{
    public  class Message:WithIdModel
    {

       

        public string Content { get; set; }

        public MessageStatus MessageStatus { get; set; }

        public MessageLanguage Language { get; set; }
        

        public MessageGroup MessageGroup { get; set; }
        public Organization Organization { get; set; }
        public Guid OrganizationId { get; set; }

        public Guid MessageGroupId { get; set; }

        public bool IsApproved { get; set; }

        public int TextSize { get; set; }

        public int NumberOfCustomer { get; set; }
             


    }
}
