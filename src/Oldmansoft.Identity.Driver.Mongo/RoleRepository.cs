using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;
using Oldmansoft.ClassicDomain.Util;

namespace Oldmansoft.Identity.Driver.Mongo
{
    class RoleRepository : ClassicDomain.Driver.Mongo.RepositoryDefinedSuperClass<Role, Domain.Role, Guid, Mapping>, Domain.IRoleRepository
    {
        public RoleRepository(UnitOfWork uow)
            : base(uow)
        { }

        public IList<Domain.Role> ListByPartitionResourceId(Guid partitionResourceId)
        {
            IQuerySupport<Domain.Role> repository = this;
            var query = repository.Query();
            return query.Where(o => o.PartitionResourceId == partitionResourceId).OrderBy(o => o.Name).ToList();
        }

        public IPagingOrdered<Domain.Role> PagingByPartitionResourceId(Guid partitionResourceId)
        {
            IQuerySupport<Domain.Role> repository = this;
            var query = repository.Query();
            return query.Paging().Where(o => o.PartitionResourceId == partitionResourceId).OrderBy(o => o.Name);
        }
    }
}
