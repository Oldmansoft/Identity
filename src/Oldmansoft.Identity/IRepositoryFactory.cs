using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;

namespace Oldmansoft.Identity
{
    public interface IRepositoryFactory
    {
        IUnitOfWork GetUnitOfWork();

        Domain.Account CreateAccountObject();

        Domain.Role CreateRoleObject();

        IRepository<Domain.Account, Guid> CreateAccountRepository();

        IRepository<Domain.Role, Guid> CreateRoleRepository();
    }
}
