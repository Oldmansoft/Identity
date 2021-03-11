using Oldmansoft.ClassicDomain;
using Oldmansoft.Identity.Domain;
using System;
using System.Collections.Generic;

namespace Oldmansoft.Identity.Infrastructure
{
    /// <summary>
    /// 帐号仓储
    /// </summary>
    public interface IAccountRepository : IRepository<Account, Guid>, IAccount
    {
        /// <summary>
        /// 从名字获取
        /// </summary>
        /// <param name="name">帐号</param>
        /// <returns></returns>
        Account GetByName(string name);

        /// <summary>
        /// 从会员序号获取
        /// </summary>
        /// <param name="memberId">成员序号</param>
        /// <returns></returns>
        Account GetByMemberId(Guid memberId);

        /// <summary>
        /// 从会员序号获取
        /// </summary>
        /// <param name="memberId">成员序号</param>
        /// <returns></returns>
        IList<Account> ListByMemberId(Guid memberId);

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="key">查找内容</param>
        /// <returns></returns>
        IPagingOrdered<Account> Paging(string key);

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="partitionResourceId">分区资源号</param>
        /// <param name="key">查找内容</param>
        /// <returns></returns>
        IPagingOrdered<Account> Paging(Guid partitionResourceId, string key);

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="index">页码</param>
        /// <param name="size">页大小</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="roleId">角色序号</param>
        /// <param name="key">查询内容</param>
        /// <returns></returns>
        IList<Account> Paging(int index, int size, out int totalCount, Guid roleId, string key);
    }
}
