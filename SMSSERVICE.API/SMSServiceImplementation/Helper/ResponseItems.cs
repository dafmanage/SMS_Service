using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Implementation.Helper
{
    public class ResponseMessage
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;

        public int ErrorCode {get;set;}

        
        public object Data { get; set; } = null!;
    }

}
