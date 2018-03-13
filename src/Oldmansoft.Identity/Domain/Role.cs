using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Domain
{
    /// <summary>
    /// 角色
    /// </summary>
    public abstract class Role
    {
        /// <summary>
        /// 序号
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// 资源分区
        /// 角色只拥有此资源下的子资源许可设定
        /// </summary>
        public Guid PartitionResourceId { get; protected set; }

        /// <summary>
        /// 授权列表
        /// </summary>
        public List<Permission> Permissions { get; protected set; }

        /// <summary>
        /// 创建角色
        /// </summary>
        protected Role()
        {
            Permissions = new List<Permission>();
        }

        /// <summary>
        /// 是否可以操作资源
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public bool HasOperator(Resource resource, Operation operation)
        {
            foreach (Permission permission in Permissions)
            {
                if (permission.HasOperator(resource, operation))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否帐号有此角色
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public abstract bool HasAccountSetIt(IAccount account);

        /// <summary>
        /// 改变
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="permissions"></param>
        public void Change(string name, string description, IEnumerable<Permission> permissions)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (description == null) throw new ArgumentNullException("description");
            if (permissions == null) throw new ArgumentNullException("permissions");
            Name = name.Trim();
            Description = description.Trim();
            Permissions.Clear();
            Permissions.AddRange(permissions);
        }

        /// <summary>
        /// 是否为相同的分区
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public bool SamePartition(Guid resourceId)
        {
            return PartitionResourceId == resourceId;
        }
    }
}
