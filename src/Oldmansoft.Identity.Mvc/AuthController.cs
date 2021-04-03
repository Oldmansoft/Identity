﻿using System;
using System.Security.Claims;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 提供用于响应对 ASP.NET MVC 网站所进行的 HTTP 请求的方法。
    /// </summary>
    public abstract class AuthController : System.Web.Mvc.Controller, Mvc.IAuthController
    {
        private const string AccountCacheKey = "IdentityAccount";

        /// <summary>
        /// 操作资源
        /// </summary>
        public abstract Guid OperateResource { get; }

        /// <summary>
        /// 身份系统仓储工厂
        /// </summary>
        protected abstract Infrastructure.IRepositoryFactory Factory { get; }

        private Guid? _AccountId;

        /// <summary>
        /// 帐号序号
        /// </summary>
        protected Guid AccountId
        {
            get
            {
                if (_AccountId.HasValue) return _AccountId.Value;

                _AccountId = Guid.Empty;
                if (User.Identity == null) return _AccountId.Value;
                var userId = GetUserId(User.Identity);
                if (string.IsNullOrEmpty(userId)) return _AccountId.Value;
                _AccountId = Guid.Parse(userId);
                return _AccountId.Value;
            }
        }

        /// <summary>
        /// 获取用户序号
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        private static string GetUserId(System.Security.Principal.IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (identity is ClaimsIdentity identity2)
            {
                return FindFirstValue(identity2, ClaimTypes.NameIdentifier);
            }
            return null;
        }

        /// <summary>
        /// 查找第一个值
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="claimType"></param>
        /// <returns></returns>
        private static string FindFirstValue(ClaimsIdentity identity, string claimType)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var claim = identity.FindFirst(claimType);
            if (claim == null)
            {
                return null;
            }
            return claim.Value;
        }

        /// <summary>
        /// 在调用操作方法前调用
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            var action = filterContext.ActionDescriptor;
            if (action.GetCustomAttributes(typeof(System.Web.Mvc.HttpPostAttribute), false).Length > 0 && action.GetCustomAttributes(typeof(CrossSiteRequestAttribute), false).Length == 0)
            {
                new CsrfDefendAttribute().OnActionExecuting(filterContext);
            }
            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// 帐号
        /// </summary>
        public Data.AccountData Account
        {
            get
            {
                if (System.Web.HttpContext.Current.Items.Contains(AccountCacheKey))
                {
                    return System.Web.HttpContext.Current.Items[AccountCacheKey] as Data.AccountData;
                }
                var result = new IdentityManager(Factory).GetAccount(AccountId);
                System.Web.HttpContext.Current.Items.Add(AccountCacheKey, result);
                return result;
            }
        }
        
        /// <summary>
        /// 是否有权限
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        protected bool HasPower(Operation operation)
        {
            return Account.HasPower(OperateResource, operation);
        }
    }
}
