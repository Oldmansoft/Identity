using System;
using System.Web;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 支持资源和操作的访问特性
    /// </summary>
    public class AuthAttribute : System.Web.Mvc.AuthorizeAttribute, Mvc.IAuthAttribute
    {
        /// <summary>
        /// 当前检测控制器
        /// </summary>
        protected AuthController Controller { get; set; }

        /// <summary>
        /// 当前方法操作符
        /// </summary>
        public Operation Operation { get; protected set; }

        /// <summary>
        /// 创建访问特性
        /// </summary>
        /// <param name="operation">操作</param>
        public AuthAttribute(Operation operation)
            : base()
        {
            Operation = operation;
        }

        /// <summary>
        /// 重写时，提供一个入口点用于进行自定义授权检查。
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext)) return false;
            
            var account = Controller.Account;
            if (account == null) return false;

            return account.HasPower(Controller.OperateResource, Operation);
        }

        /// <summary>
        /// 在过程请求授权时调用。
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (!(filterContext.Controller is AuthController))
            {
                throw new IdentityException("使用 Auth 特性必须建立在 AuthController 里面，请继承它。");
            }
            Controller = filterContext.Controller as AuthController;
            base.OnAuthorization(filterContext);
        }
    }
}
