using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;
using Oldmansoft.ClassicDomain.Driver.Mongo;
using Oldmansoft.Identity.Infrastructure;

namespace Oldmansoft.Identity.Driver.Mongo
{
    /// <summary>
    /// 仓储工厂
    /// </summary>
    public class RepositoryFactory : IRepositoryFactory
    {
        private UnitOfWork Uow { get; set; }

        /// <summary>
        /// 仓储仓储工厂
        /// </summary>
        public RepositoryFactory()
        {
            Uow = new UnitOfWork();
        }

        /// <summary>
        /// 获取工作单元
        /// </summary>
        /// <returns></returns>
        public IUnitOfWork GetUnitOfWork()
        {
            return Uow;
        }

        /// <summary>
        /// 创建帐号对象
        /// </summary>
        /// <param name="partitionResourceId">分区资源号</param>
        /// <param name="name">名称</param>
        /// <param name="memberType">会员类型</param>
        /// <returns></returns>
        public Domain.Account CreateAccountObject(Guid partitionResourceId, string name, int memberType)
        {
            return Account.Create(partitionResourceId, name, memberType);
        }

        /// <summary>
        /// 创建角色对象
        /// </summary>
        /// <param name="partitionResourceId">资源区</param>
        /// <param name="name">名称</param>
        /// <param name="description">注释</param>
        /// <param name="permissions">许可列表</param>
        /// <returns></returns>
        public Domain.Role CreateRoleObject(Guid partitionResourceId, string name, string description, List<Domain.Permission> permissions)
        {
            return Role.Create(partitionResourceId, name, description, permissions);
        }

        /// <summary>
        /// 创建帐号仓储
        /// </summary>
        /// <returns></returns>
        public IAccountRepository CreateAccountRepository()
        {
            return new AccountRepository(Uow);
        }

        /// <summary>
        /// 创建角色仓储
        /// </summary>
        /// <returns></returns>
        public IRoleRepository CreateRoleRepository()
        {
            return new RoleRepository(Uow);
        }
    }
}
