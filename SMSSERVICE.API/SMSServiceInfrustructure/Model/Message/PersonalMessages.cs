using IntegratedInfrustructure.Model.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;

namespace SMSServiceInfrustructure.Model.Message
{
    public  class PersonalMessages :WithIdModel
    {

        public Guid MessageId { get; set; }
        public virtual Message Message { get; set; }        
        public string PhoneNumber { get; set; }

        public MessageStatus MessageStatus { get; set; }




    }


}
