using Oldmansoft.ClassicDomain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oldmansoft.Identity.Driver.Mongo
{
    class RoleRepository : ClassicDomain.Driver.Mongo.RepositoryDefinedSuperClass<Role, Domain.Role, Guid, Mapping>, Infrastructure.IRoleRepository
    {
        public IList<Domain.Role> ListByPartitionResourceId(Guid partitionResourceId)
        {
            IQuerySupport<Domain.Role> repository = this;
            var query = repository.Query();
            return query.Where(o => o.PartitionResourceId == partitionResourceId).OrderBy(o => o.Name).ToList();
        }

        public IPagingOrdered<Domain.Role> PagingByPartitionResourceId(Guid partitionResourceId, string key)
        {
            IQuerySupport<Domain.Role> repository = this;
            var query = repository.Query();
            if (string.IsNullOrWhiteSpace(key))
            {
                return query.Paging().Where(o => o.PartitionResourceId == partitionResourceId).OrderByDescending(o => o.Id);
            }
            else
            {
                return query.Paging().Where(o => o.PartitionResourceId == partitionResourceId && o.Name.StartsWith(key.Trim())).OrderByDescending(o => o.Id);
            }
        }
    }
}
