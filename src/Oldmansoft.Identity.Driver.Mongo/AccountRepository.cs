using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;
using Oldmansoft.ClassicDomain.Util;

namespace Oldmansoft.Identity.Driver.Mongo
{
    class AccountRepository : ClassicDomain.Driver.Mongo.RepositoryDefinedSuperClass<Account, Domain.Account, Guid, Mapping>, Domain.IAccountRepository
    {
        public AccountRepository(UnitOfWork uow)
            : base(uow)
        { }

        public bool ContainsRoleId(Guid roleId)
        {
            return Query().Where(o => o.RoleIds.Contains(roleId)).FirstOrDefault() != null;
        }

        public Domain.Account GetByMemberId(Guid memberId)
        {
            return Query().Where(o => o.MemberId == memberId).FirstOrDefault();
        }

        public Domain.Account GetByName(string name)
        {
            return Query().Where(o => o.Name == name).FirstOrDefault();
        }

        public IPagingOrdered<Domain.Account> Paging()
        {
            IQuerySupport<Domain.Account> repository = this;
            var query = repository.Query();
            return query.Paging().OrderByDescending(o => o.CreatedTime);
        }
    }
}
