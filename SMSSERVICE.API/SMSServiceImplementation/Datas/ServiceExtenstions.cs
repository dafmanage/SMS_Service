using Implementation.Interfaces.Authentication;
using Implementation.Services.Authentication;
using IntegratedImplementation.Interfaces.Configuration;
using IntegratedImplementation.Interfaces.HRM;
using IntegratedImplementation.Services.Configuration;
using IntegratedImplementation.Services.HRM;
using Microsoft.Extensions.DependencyInjection;
using SMSServiceImplementation.Interfaces.Message;
using SMSServiceImplementation.Interfaces.Report;
using SMSServiceImplementation.Services.Message;
using SMSServiceImplementation.Services.Report;

namespace IntegratedImplementation.Datas
{
    public static class ServiceExtenstions
    {
        public static IServiceCollection AddCoreBusiness(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            //hrm services 
         
            services.AddScoped<IGeneralConfigService, GeneralConfigService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IMessageGroupService, MessageGroupService>();
            services.AddScoped<IGroupPhoneService, GroupPhoneService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IReport, ReportService>();
  
            return services;
        }
    }
}
