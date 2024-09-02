using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegratedImplementation.DTOS.Authentication
{
    public class ApplicationSetting
    {
        public string JWT_Secret { get; set; } = null!;
        public string Client_URL { get; set; } = null!;
    }
}
