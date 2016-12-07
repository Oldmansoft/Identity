using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 身份异常
    /// </summary>
    public class IdentityException : Exception
    {
        public IdentityException(string message)
            : base(message)
        {
        }
    }
}
