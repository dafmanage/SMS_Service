using IntegratedInfrustructure.Model.HRM;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;

namespace IntegratedImplementation.DTOS.HRM
{
    public record OrganizationPostDto
    {

        public Guid? Id { get; set; }
        public string Name { get; set; } = null!;
        public string NameLocal { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public IFormFile? Image { get; set; }
        public string? ImagePath { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string OrganizationStatus { get; set; }= null!;
        public string CreatedById { get; set; } = null!;


    }

    public class OrganizationGetDto
    {
       
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string NameLocal { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public IFormFile? Image { get; set; }
        public string? ImagePath { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string OrganizationStatus { get; set; }




    }

 

}
