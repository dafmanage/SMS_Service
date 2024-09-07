using AutoMapper;
using AutoMapper.QueryableExtensions;
using Implementation.Helper;
using IntegratedImplementation.DTOS.Configuration;
using IntegratedImplementation.DTOS.HRM;
using IntegratedImplementation.Interfaces.Configuration;
using IntegratedImplementation.Interfaces.HRM;
using IntegratedInfrustructure.Data;
using IntegratedInfrustructure.Model.Authentication;
using IntegratedInfrustructure.Model.HRM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;

namespace IntegratedImplementation.Services.HRM
{
    public class OrganizationService : IOrganizationService
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly IGeneralConfigService _generalConfig;
        private UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public OrganizationService(ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager, 
            IGeneralConfigService generalConfig, IMapper mapper)
        {
            _dbContext = dbContext;
            _generalConfig = generalConfig;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ResponseMessage> Addorganization(OrganizationPostDto addorganization)
        {
            var id = Guid.NewGuid();
            var path = "";

            //if (addorganization.Image != null)
            //    path = _generalConfig.UploadFiles(addorganization.Image, id.ToString(), "organization").Result.ToString();
            if (addorganization.Image != null)
            {
                if (!IsValidFileType(addorganization.Image))
                {
                    return new ResponseMessage
                    {
                        Message = "Invalid file type.",
                        Success = false
                    };
                }

          
          
                    path = _generalConfig.UploadFiles(addorganization.Image, id.ToString(), "organization").Result.ToString();
          
            }


            addorganization.OrganizationStatus = EmploymentStatus.ACTIVE.ToString();
            Organization organization = new Organization
            {
                Id = id,
                CreatedDate = DateTime.Now,
                CreatedById = addorganization.CreatedById,
                Name = addorganization.Name,
                NameLocal =addorganization.NameLocal,
                Email = addorganization.Email,
                OrganizationStatus = Enum.Parse<OrganizationStatus>(addorganization.OrganizationStatus),              
                Address = addorganization.Address,
                ImagePath = path,
                PhoneNumber = addorganization.PhoneNumber,        
                Rowstatus = RowStatus.ACTIVE,

            };
            await _dbContext.Organizations.AddAsync(organization);
            await _dbContext.SaveChangesAsync();



            return new ResponseMessage
            {

                Message = "Added Successfully",
                Success = true
            };
        }
        private bool IsValidFileType(IFormFile file)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/svg+xml" };
            return allowedTypes.Contains(file.ContentType);
        }




        public async Task<ResponseMessage> Updateorganization(OrganizationGetDto addorganization)
        {

            var path = "";


            if (addorganization.Image != null)
            {
                if (!IsValidFileType(addorganization.Image))
                {
                    return new ResponseMessage
                    {
                        Message = "Invalid file type.",
                        Success = false
                    };
                }



                path = _generalConfig.UploadFiles(addorganization.Image, addorganization.Id.ToString(), "organization").Result.ToString();

            }





            var organization = _dbContext.Organizations.Find(addorganization.Id);

            if (organization != null)
            {

                organization.Email = addorganization.Email;
                organization.OrganizationStatus = Enum.Parse<OrganizationStatus>(addorganization.OrganizationStatus);
                organization.Name =addorganization.Name;
                organization.NameLocal = addorganization.NameLocal;
                organization.Address = addorganization.Address;
               
                
                

                if (addorganization.Image!=null)
                {
                    organization.ImagePath = path;
                }
                organization.PhoneNumber = addorganization.PhoneNumber;

               

                organization.Rowstatus = RowStatus.ACTIVE;

                await _dbContext.SaveChangesAsync();

                return new ResponseMessage
                {

                    Message = "Updated Successfully",
                    Success = true
                };

            }
            else
            {
                return new ResponseMessage
                {

                    Message = "No organization Found",
                    Success = false
                };
            }
   



        }

        public async Task<List<OrganizationGetDto>> Getorganizations()
        {
            var organizationList = await _dbContext.Organizations.AsNoTracking()
                                    .ProjectTo<OrganizationGetDto>(_mapper.ConfigurationProvider)
                                    .ToListAsync();
            return organizationList;
        }

        public async Task<OrganizationGetDto> Getorganization(Guid organizationId)
        {
            var organization = await _dbContext.Organizations

                .Where(x => x.Id == organizationId)
                .AsNoTracking()
                .ProjectTo<OrganizationGetDto>(_mapper.ConfigurationProvider).FirstAsync();

            return organization;
        }



        public async Task<List<SelectListDto>> GetorganizationNoUser()
        {
            var users = _userManager.Users.Select(x => x.OrganizationId).ToList();
                
            var organizations = await _dbContext.Organizations
                .Where(e => !users.Contains(e.Id))
                .ProjectTo<SelectListDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return organizations;
        }
        

      public async  Task<List<SelectListDto>> GetorganizationSelectList()
        {

            var organizations = await _dbContext.Organizations.ProjectTo<SelectListDto>(_mapper.ConfigurationProvider).ToListAsync();
            
            return organizations;
        }

        public async Task<ResponseMessage> changeorganizationImage(OrganizationPostDto addorganization)
        {

            var path = "";

            if (addorganization.Image != null)
                path = _generalConfig.UploadFiles(addorganization.Image, addorganization.Id.ToString(), "organization").Result.ToString();
            var organization = _dbContext.Organizations.Find(addorganization.Id);
            if (organization != null)
            {
                if (addorganization.Image != null)
                {
                    organization.ImagePath = path;
                }

                await _dbContext.SaveChangesAsync();

                return new ResponseMessage
                {

                    Message = "Image Updated Successfully",
                    Success = true
                };

            }
            else
            {
                return new ResponseMessage
                {

                    Message = "No organization Found",
                    Success = false
                };
            }

        }
    }
}
