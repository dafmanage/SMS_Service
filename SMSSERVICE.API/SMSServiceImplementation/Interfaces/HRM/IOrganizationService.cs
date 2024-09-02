using Implementation.Helper;
using IntegratedImplementation.DTOS.Configuration;
using IntegratedImplementation.DTOS.HRM;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegratedImplementation.Interfaces.HRM
{
    public interface IOrganizationService
    {

        Task<List<OrganizationGetDto>> Getorganizations();
        Task<ResponseMessage> Addorganization(OrganizationPostDto addorganization);
        Task<ResponseMessage> Updateorganization(OrganizationGetDto addorganization);
        Task<OrganizationGetDto> Getorganization(Guid organizationId);
        Task<List<SelectListDto>> GetorganizationNoUser();
        Task<List<SelectListDto>> GetorganizationSelectList();
        Task<ResponseMessage> changeorganizationImage(OrganizationPostDto addorganization);




    }
}
