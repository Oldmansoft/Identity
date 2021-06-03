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
            var referer = filterContext.HttpContext.Request.UrlReferrer;
            var url = filterContext.HttpContext.Request.Url;
            if (referer == null)
            {
                SetDefendContent(filterContext);
                return;
            }

            if (referer.Host != url.Host || referer.Port != url.Port)
            {
                SetDefendContent(filterContext);
                return;
            }
        }

        private void SetDefendContent(ActionExecutingContext filterContext)
        {
            var content = new ContentResult
            {
                Content = "^_^",
                ContentType = "text/plain"
            };
            filterContext.Result = content;
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}
