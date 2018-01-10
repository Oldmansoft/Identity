using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;

namespace Oldmansoft.Identity.Driver.Mongo
{
    class Mapping : ClassicDomain.Driver.Mongo.Context
    {
        protected override void OnModelCreating()
        {
            Add<Account, Guid>(domain => domain.Id)
                .SetIndex(domain => domain.PartitionResourceId)
                .SetUnique(domain => domain.Name)
                .SetIndex(domain => domain.MemberId)
                .SetIndex(domain => domain.RoleIds);
            Add<Role, Guid>(domain => domain.Id)
                .SetIndex(g => g.CreateGroup(domain => domain.PartitionResourceId).Add(domain => domain.Name));
        }
    }
}
