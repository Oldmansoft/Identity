using System;
using System.Collections.Generic;
using System.Text;

namespace Oldmansoft.Identity.Mvc
{
    /// <summary>
    /// 支持资源和操作的访问特性
    /// </summary>
    public interface IAuthAttribute
    {
        /// <summary>
        /// 当前方法操作符
        /// </summary>
        Operation Operation { get; }
    }
}
