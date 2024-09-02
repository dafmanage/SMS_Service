using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServiceImplementation.DTOS.Message
{
    public record GroupPhonePostDto
    {
        public Guid MessageGroupId { get; set; }  
        
        public string ? MessageGrpup{ get; set; }
        public string? ErrorMessage { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string? Remark { get; set; }
        public string CreatedById { get; set; } 
    }
    public record GroupPhoneGetDto
    {
        public Guid Id { get; set; }    
        public Guid MessageGroupId { get; set; }

        public string MessageGroup { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string? Remark { get; set; }
    }
}
