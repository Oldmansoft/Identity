using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;

namespace Oldmansoft.Identity.Domain
{
    /// <summary>
    /// 帐号仓储
    /// </summary>
    public interface IAccountRepository : IRepository<Account, Guid>
    {
        /// <summary>
        /// 从名字获取
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Account GetByName(string name);

        /// <summary>
        /// 从会员序号获取
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        Account GetByMemberId(Guid memberId);

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="key">查找内容</param>
        /// <returns></returns>
        IPagingOrdered<Account> Paging(string key);

        /// <summary>
        /// 是否包含角色序号
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        bool ContainsRoleId(Guid roleId);
    }
}
