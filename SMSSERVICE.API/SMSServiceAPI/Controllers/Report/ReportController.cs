using AutoMapper;
using AutoMapper.QueryableExtensions;
using IntegratedInfrustructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMSServiceImplementation.DTOS.Message;
using SMSServiceImplementation.DTOS.Report;
using SMSServiceImplementation.Interfaces.Report;
using System.Net;

namespace SMSServiceAPI.Controllers.Report
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReport report;
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;

        public ReportController(IReport report, ApplicationDbContext dbContext, IMapper mapper)
        {
            this.report = report;
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetReportDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessageGroups(Guid messageGroupId)
        //public async Task<IEnumerable<GetReportDto>> GetReportByMessageGroupId(Guid messageGroupId)
        {
            return Ok(await report.GetReports(messageGroupId));
            //var results = await dbContext.Messages
            //    .Include(m => m.MessageGroup)
            //    .Include(m => m.Organization)
            //    .Where(m => m.MessageGroupId == messageGroupId)
            //    // ... join with other tables as needed
            //    .ProjectTo<GetReportDto>(mapper.ConfigurationProvider)
            //    .ToListAsync();

            //return results;
        }
    }
}
