using System;
using System.Collections.Generic;
using System.Text;

namespace Oldmansoft.Identity.Mvc
{
    /// <summary>
    /// 提供用于响应对 ASP.NET MVC 网站所进行的 HTTP 请求的方法。
    /// </summary>
    public interface IAuthController
    {
        /// <summary>
        /// 操作资源
        /// </summary>
        Guid OperateResource { get; }
    }
}
