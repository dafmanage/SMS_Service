using AutoMapper;
using IntegratedImplementation.DTOS.HRM;
using IntegratedInfrustructure.Model.HRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;

using IntegratedInfrustructure.Data;
using IntegratedImplementation.DTOS.Configuration;
using SMSServiceInfrustructure.Model.Message;
using SMSServiceImplementation.DTOS.Message;
using SMSServiceInfrustructure.Model.Report;
using SMSServiceImplementation.DTOS.Report;

namespace IntegratedImplementation.Datas
{
    public class AutoMapperConfigurations : Profile
    {

        public AutoMapperConfigurations()
        {

            CreateMap<Organization, OrganizationGetDto>()                
                .ForMember(a => a.OrganizationStatus, e => e.MapFrom(mfg => mfg.OrganizationStatus.ToString()));


            CreateMap<MessageGroup, MessageGroupGetDto>()
                .ForMember(a => a.OrganizationName, e => e.MapFrom(mfg => mfg.Organization.Name));

            CreateMap<MessageGroupPhone, GroupPhoneGetDto>()
                .ForMember(a => a.MessageGroup, e => e.MapFrom(mfg => mfg.MessageGroup.GroupName));

            



            CreateMap<Organization, SelectListDto>()
               .ForMember(a => a.Id, e => e.MapFrom(mfg => mfg.Id))
               .ForMember(a => a.Name, e => e.MapFrom(mfg => $"{mfg.Name} {mfg.NameLocal}"));


            CreateMap<Report, GetReportDto>()
                .ForMember(a => a.GroupName, e => e.MapFrom(mfg => mfg.MessageGroup.GroupName))
                .ForMember(a => a.NumberOfCustomer, e => e.MapFrom(mfg => mfg.Message.NumberOfCustomer))
                .ForMember(a => a.Content, e => e.MapFrom(mfg => mfg.Message.Content));

            //CreateMap<Message, GetReportDto>();
        }
    }
}
