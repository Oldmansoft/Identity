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

        public IPagingOrdered<Domain.Account> Paging(string key)
        {
            IQuerySupport<Domain.Account> repository = this;
            var query = repository.Query();
            if (string.IsNullOrWhiteSpace(key))
            {
                return query.Paging().OrderByDescending(o => o.Id);
            }
            else
            {
                return query.Paging().Where(o => o.Name.StartsWith(key.Trim())).OrderByDescending(o => o.Id);
            }
        }
        
        public IPagingOrdered<Domain.Account> Paging(Guid partitionResourceId, string key)
        {
            IQuerySupport<Domain.Account> repository = this;
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

        public IList<Domain.Account> Paging(int index, int size, out int totalCount, Guid roleId, string key)
        {
            IPagingCondition<Account> condition;
            if (string.IsNullOrWhiteSpace(key))
            {
                condition = Query().Paging().Where(o => o.RoleIds.Contains(roleId));
            }
            else
            {
                condition = Query().Paging().Where(o => o.RoleIds.Contains(roleId) && o.Name.StartsWith(key.Trim()));
            }

            var result = new List<Domain.Account>();
            foreach(var item in condition.OrderByDescending(o => o.Id).Size(size).ToList(index, out totalCount))
            {
                result.Add(item);
            }
            return result;
        }
    }
}
