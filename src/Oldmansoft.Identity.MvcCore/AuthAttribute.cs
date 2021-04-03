using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 支持资源和操作的访问特性
    /// </summary>
    public class AuthAttribute : Attribute, IActionFilter, Mvc.IAuthAttribute
    {
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

        void IActionFilter.OnActionExecuted(ActionExecutedContext context)
        {
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.Controller is AuthController))
            {
                throw new IdentityException("使用 Auth 特性必须建立在 AuthController 里面，请继承它。");
            }
            var controller = context.Controller as AuthController;
            var account = controller.Account;
            if (account != null && account.HasPower(controller.OperateResource, Operation)) return;

            context.Result = new ObjectResult("权限不足");
        }
    }
}
