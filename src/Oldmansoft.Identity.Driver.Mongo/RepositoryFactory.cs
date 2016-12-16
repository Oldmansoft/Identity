using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;
using Oldmansoft.ClassicDomain.Driver.Mongo;

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
        /// <returns></returns>
        public Domain.Account CreateAccountObject()
        {
            return new Account();
        }

        /// <summary>
        /// 创建角色对象
        /// </summary>
        /// <returns></returns>
        public Domain.Role CreateRoleObject()
        {
            return new Role();
        }

        /// <summary>
        /// 创建帐号仓储
        /// </summary>
        /// <returns></returns>
        public IRepository<Domain.Account, Guid> CreateAccountRepository()
        {
            return new RepositoryDefinedSuperClass<Account, Domain.Account, Guid, Mapping>(Uow);
        }

        /// <summary>
        /// 创建角色仓储
        /// </summary>
        /// <returns></returns>
        public IRepository<Domain.Role, Guid> CreateRoleRepository()
        {
            return new RepositoryDefinedSuperClass<Role, Domain.Role, Guid, Mapping>(Uow);
        }
    }
}
