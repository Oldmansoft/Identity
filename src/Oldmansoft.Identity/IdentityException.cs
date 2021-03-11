using System;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 身份异常
    /// </summary>
    public class IdentityException : Exception
    {
        /// <summary>
        /// 创建身份异常
        /// </summary>
        /// <param name="message"></param>
        public IdentityException(string message)
            : base(message)
        {
        }
    }
}
