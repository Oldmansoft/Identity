using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;
using Oldmansoft.Identity.Domain;

namespace Oldmansoft.Identity.Infrastructure
{
    /// <summary>
    /// 仓储工厂
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// 获取工作单元
        /// </summary>
        /// <returns></returns>
        IUnitOfWork GetUnitOfWork();

        /// <summary>
        /// 创建帐号对象
        /// </summary>
        /// <param name="partitionResourceId">分区资源号</param>
        /// <param name="name">名称</param>
        /// <param name="memberType">会员类型</param>
        /// <returns></returns>
        Account CreateAccountObject(Guid partitionResourceId, string name, int memberType);

        /// <summary>
        /// 创建角色对象
        /// </summary>
        /// <param name="partitionResourceId">资源区</param>
        /// <param name="name">名称</param>
        /// <param name="description">注释</param>
        /// <param name="permissions">许可列表</param>
        /// <returns></returns>
        Role CreateRoleObject(Guid partitionResourceId, string name, string description, List<Permission> permissions);

        /// <summary>
        /// 创建帐号仓储
        /// </summary>
        /// <returns></returns>
        IAccountRepository CreateAccountRepository();

        /// <summary>
        /// 创建角色仓储
        /// </summary>
        /// <returns></returns>
        IRoleRepository CreateRoleRepository();
    }
}
