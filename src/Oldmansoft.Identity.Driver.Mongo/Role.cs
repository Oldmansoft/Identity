using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Driver.Mongo
{
    class Role : Domain.Role
    {
        public override bool HasAccountSetIt(IQueryable<Domain.Account> query)
        {
            var q = query as IQueryable<Account>;
            return q.Where(o => o.RoleIds.Contains(Id)).FirstOrDefault() != null;
        }
    }
}
