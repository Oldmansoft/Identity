using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Driver.Mongo
{
    class Role : Domain.Role
    {
        private Role() { }

        public static Role Create(Guid partitionResourceId, string name, string description, IList<Domain.Permission> permissions)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (description == null) throw new ArgumentNullException("description");
            if (permissions == null) throw new ArgumentNullException("permissions");

            var result = new Role();
            result.PartitionResourceId = partitionResourceId;
            result.Name = name.Trim();
            result.Description = description.Trim();
            result.Permissions.AddRange(permissions);
            return result;
        }

        public override bool HasAccountSetIt(Domain.IAccount account)
        {
            return account.ContainsRoleId(Id);
        }
    }
}
