using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// CSRF 防御
    /// </summary>
    public class CsrfDefendAttribute : Attribute, IActionFilter
    {
        /// <summary>
        /// 在执行操作方法之前调用。
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var referer = request.GetTypedHeaders().Referer;
            if (referer == null)
            {
                SetDefendContent(context);
                return;
            }

            if (referer.Host != request.Host.Host)
            {
                SetDefendContent(context);
                return;
            }
            if (request.Host.Port.HasValue && referer.Port != request.Host.Port)
            {
                SetDefendContent(context);
                return;
            }
        }

        private void SetDefendContent(ActionExecutingContext context)
        {
            var content = new ContentResult
            {
                Content = "^_^",
                ContentType = "text/plain"
            };
            context.Result = content;
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
