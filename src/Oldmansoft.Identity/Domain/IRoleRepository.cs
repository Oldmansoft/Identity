using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;

namespace Oldmansoft.Identity.Domain
{
    /// <summary>
    /// 角色仓储
    /// </summary>
    public interface IRoleRepository : IRepository<Role, Guid>
    {
        /// <summary>
        /// 根据分区资源列出角色
        /// </summary>
        /// <param name="partitionResourceId"></param>
        /// <returns></returns>
        IList<Role> ListByPartitionResourceId(Guid partitionResourceId);

        /// <summary>
        /// 根据分区资源分页列出角色
        /// </summary>
        /// <param name="partitionResourceId"></param>
        /// <returns></returns>
        IPagingOrdered<Role> PagingByPartitionResourceId(Guid partitionResourceId);
    }
}
