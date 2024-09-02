using Implementation.Helper;
using Microsoft.AspNetCore.Http;
using SMSServiceImplementation.DTOS.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServiceImplementation.Interfaces.Message
{
    public interface IGroupPhoneService
    {
        Task<List<GroupPhoneGetDto>> GetGroupPhones(Guid MessageGroupId);
        Task<ResponseMessage> AddGroupPhone(GroupPhonePostDto addGroupPhone);

        Task<ResponseMessage> AddGroupPhoneFromExcel(IFormFile ExcelFile,string createdById);
        Task<ResponseMessage> UpdateGroupPhone(GroupPhoneGetDto addGroupPhone);

    }
}
