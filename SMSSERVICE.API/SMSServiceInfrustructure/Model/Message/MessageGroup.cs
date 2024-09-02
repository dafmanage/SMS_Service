using IntegratedInfrustructure.Model.Authentication;
using IntegratedInfrustructure.Model.HRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServiceInfrustructure.Model.Message
{
    public class MessageGroup : WithIdModel
    {
        public MessageGroup()
        {

            GroupPhoneNumbers = new HashSet<MessageGroupPhone>();

        }

        public string GroupName { get; set; }

        public string GroupCode { get; set; }

        public string Remark { get; set; }

        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }


        public ICollection<MessageGroupPhone> GroupPhoneNumbers { get; set; }

    }
}
