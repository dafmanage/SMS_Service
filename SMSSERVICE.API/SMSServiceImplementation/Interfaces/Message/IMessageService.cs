using Implementation.Helper;
using IntegratedInfrustructure.Model.HRM;
using SMSServiceImplementation.DTOS.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServiceImplementation.Interfaces.Message
{
    public interface IMessageService
    {

        Task<List<MessageGetDto>> GetMessages(Guid messageGroupId);
        Task<ResponseMessage> AddMessages(MessagePostDto addMessages);
        Task<List<MessageGetDto>> GetUnsentMessages(Guid?  organizationId);
    }
}
