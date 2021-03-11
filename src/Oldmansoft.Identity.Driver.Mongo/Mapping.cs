using Oldmansoft.ClassicDomain;
using System;

namespace Oldmansoft.Identity.Driver.Mongo
{
    /// <summary>
    /// Mongo Context 映射文件
    /// </summary>
    class Mapping : ClassicDomain.Driver.Mongo.Context
    {
        /// <summary>
        /// 在模型创建时
        /// </summary>
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
