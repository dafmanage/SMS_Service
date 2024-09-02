using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegratedImplementation.DTOS.Configuration
{
    public class SelectListDto
    {

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public string ? ImagePath{ get; set; }
    }
}
