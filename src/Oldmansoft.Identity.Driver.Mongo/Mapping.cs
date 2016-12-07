using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Driver.Mongo
{
    class Mapping : ClassicDomain.Driver.Mongo.Context
    {
        protected override void OnModelCreating()
        {
            Add<Account, Guid>(domain => domain.Id)
                .SetUnique(domain => domain.Name)
                .SetIndex(domain => domain.MemberId)
                .SetIndex(domain => domain.RoleIds)
                .SetIndex(domain => domain.CreatedTime);
            Add<Role, Guid>(domain => domain.Id)
                .SetIndex(domain => domain.PartitionResourceId);
        }
    }
}
