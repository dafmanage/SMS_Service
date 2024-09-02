using SMSServiceImplementation.DTOS.Message;
using SMSServiceImplementation.DTOS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSServiceImplementation.Interfaces.Report
{
    public interface IReport
    {
        Task<List<GetReportDto>> GetReports(Guid OrganizationId);
    }
}
