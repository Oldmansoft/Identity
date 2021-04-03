using Oldmansoft.ClassicDomain;
using Oldmansoft.Identity.Infrastructure;
using System;
using System.Collections.Generic;

namespace Oldmansoft.Identity.Driver.Mongo
{
    /// <summary>
    /// 仓储工厂
    /// </summary>
    public class RepositoryFactory : ClassicDomain.RepositoryFactory, Infrastructure.IRepositoryFactory
    {
        static RepositoryFactory()
        {
            Add<IAccountRepository, AccountRepository>();
            Add<IRoleRepository, RoleRepository>();
        }

        /// <summary>
        /// 设置连接字符串
        /// </summary>
        /// <param name="connectionString"></param>
        public static void SetConnectionString(string connectionString)
        {
            ConnectionString.Set<Mapping>(connectionString);
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
    }
}
