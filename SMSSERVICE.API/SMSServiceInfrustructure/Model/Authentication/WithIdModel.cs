using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntegratedInfrustructure.Data.EnumList;

namespace IntegratedInfrustructure.Model.Authentication
{
    public class WithIdModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
       
        public string CreatedById { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
    
        public RowStatus Rowstatus { get; set; }
    }
}
