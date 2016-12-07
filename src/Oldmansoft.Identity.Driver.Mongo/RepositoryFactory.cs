using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;
using Oldmansoft.ClassicDomain.Driver.Mongo;

namespace Oldmansoft.Identity.Driver.Mongo
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private UnitOfWork Uow { get; set; }

        public RepositoryFactory()
        {
            Uow = new UnitOfWork();
        }

        public IUnitOfWork GetUnitOfWork()
        {
            return Uow;
        }
        
        public Domain.Account CreateAccountObject()
        {
            return new Account();
        }

        public Domain.Role CreateRoleObject()
        {
            return new Role();
        }

        public IRepository<Domain.Account, Guid> CreateAccountRepository()
        {
            return new RepositoryDefinedSuperClass<Account, Domain.Account, Guid, Mapping>(Uow);
        }

        public IRepository<Domain.Role, Guid> CreateRoleRepository()
        {
            return new RepositoryDefinedSuperClass<Role, Domain.Role, Guid, Mapping>(Uow);
        }
    }
}
