using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Driver.Mongo
{
    class Role : Domain.Role
    {
        public override bool HasAccountSetIt(Domain.IAccountRepository accountRepository)
        {
            return accountRepository.ContainsRoleId(Id);
        }
    }
}
