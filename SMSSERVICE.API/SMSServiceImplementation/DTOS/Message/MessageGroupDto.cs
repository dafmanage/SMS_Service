using IntegratedInfrustructure.Model.HRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServiceImplementation.DTOS.Message
{
    public record MessageGroupPostDto
    {
        public string GroupName { get; set; }

        public string GroupCode { get; set; }

        public string Remark { get; set; }

        public Guid OrganizationId { get; set; }

        public string CreatedById { get; set; }
    }
    public record MessageGroupGetDto
    {

        public Guid ? Id { get; set; }
        public string GroupName { get; set; }
        public string GroupCode { get; set; }
        public string Remark { get; set; }

        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }

    }
}
