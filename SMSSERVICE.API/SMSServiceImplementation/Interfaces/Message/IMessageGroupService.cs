using Implementation.Helper;
using IntegratedImplementation.DTOS.HRM;
using SMSServiceImplementation.DTOS.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServiceImplementation.Interfaces.Message
{
    public interface IMessageGroupService
    {

        Task<List<MessageGroupGetDto>> GetMessageGroups(Guid OrganizationId);
        Task<ResponseMessage> AddMessageGroup(MessageGroupPostDto addMessageGroup);
        Task<ResponseMessage> UpdateMessageGroup(MessageGroupGetDto addMessageGroup);
    }
}
