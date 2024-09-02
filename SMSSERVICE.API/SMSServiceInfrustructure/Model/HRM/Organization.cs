using IntegratedInfrustructure.Model.Authentication;
using IntegratedInfrustructure.Model.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;

namespace IntegratedInfrustructure.Model.HRM
{
    public class Organization : WithIdModel
    {
             
       
        public string Name { get; set; } = null!;
        public string NameLocal { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ImagePath { get; set; } = null!;
        public string Address { get; set; } = null!;     
        public OrganizationStatus OrganizationStatus { get; set; }
       

    }

    
}
