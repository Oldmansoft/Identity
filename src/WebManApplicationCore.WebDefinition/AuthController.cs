using Oldmansoft.Identity;
using Oldmansoft.Identity.Infrastructure;

namespace WebManApplicationCore
{
    /// <summary>
    /// 带身份验证功能的控制器
    /// </summary>
    public abstract class AuthController : Oldmansoft.Identity.AuthController
    {
        /// <summary>
        /// 身份系统工厂
        /// </summary>
        protected override IRepositoryFactory Factory
        {
            get
            {
                return new Oldmansoft.Identity.Driver.Mongo.RepositoryFactory();
            }
        }

        /// <summary>
        /// 创建身份访问管理器
        /// </summary>
        /// <returns></returns>
        protected IdentityManager CreateIdentity()
        {
            return new IdentityManager(Factory);
        }

        /// <summary>
        /// 返回内容结果
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected Oldmansoft.Html.WebMan.HtmlResult Content(Oldmansoft.Html.IHtmlElement element)
        {
            return new Oldmansoft.Html.WebMan.HtmlResult(element);
        }
    }
}
