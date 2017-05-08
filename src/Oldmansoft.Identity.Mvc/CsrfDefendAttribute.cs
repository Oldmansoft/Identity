using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// CSRF 防御
    /// </summary>
    public class CsrfDefendAttribute : FilterAttribute, IActionFilter
    {
        /// <summary>
        /// 在执行操作方法之前调用。
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var referrer = filterContext.HttpContext.Request.UrlReferrer;
            var url = filterContext.HttpContext.Request.Url;
            if (referrer == null)
            {
                SetDefendContent(filterContext);
                return;
            }

            if (referrer.Host != url.Host || referrer.Port != url.Port)
            {
                SetDefendContent(filterContext);
                return;
            }
        }

        private void SetDefendContent(ActionExecutingContext filterContext)
        {
            var content = new ContentResult();
            content.Content = "^_^";
            content.ContentType = "text/plain";
            filterContext.Result = content;
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}
