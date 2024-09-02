using IntegratedInfrustructure.Model.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SMSServiceInfrustructure.Model.Message
{
    public class MessageGroupPhone : WithIdModel
    {
        public Guid MessageGroupId { get; set; }

        public virtual MessageGroup MessageGroup { get; set; }

        [MinLength(9, ErrorMessage = "Phone number must be at least 9 digits.")]
        public string PhoneNumber { get; set; }

        public string FullName { get; set; }

        public string? Remark { get; set; }




    }

}
