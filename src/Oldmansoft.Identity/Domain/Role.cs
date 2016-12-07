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
        public Guid Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 资源分区
        /// 角色只拥有此资源下的子资源许可设定
        /// </summary>
        public Guid PartitionResourceId { get; set; }

        /// <summary>
        /// 授权列表
        /// </summary>
        public List<Permission> Permissions { get; set; }

        /// <summary>
        /// 创建角色
        /// </summary>
        public Role()
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

        public abstract bool HasAccountSetIt(IQueryable<Account> query);
    }
}
