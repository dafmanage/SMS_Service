using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSServiceImplementation.Interfaces.Authentication
{
    public interface ITokenValidator
    {
        public interface ITokenValidator
        {
            Task<bool> IsTokenBlacklisted(string token);
        }

    }
}