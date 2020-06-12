using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Data
{
    /// <summary>
    /// 帐号数据
    /// </summary>
    public class AccountData
    {
        /// <summary>
        /// 序号
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 资源分区
        /// 用于区分帐号管理区域
        /// </summary>
        public Guid PartitionResourceId { get; set; }

        /// <summary>
        /// 帐号名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 成员序号
        /// </summary>
        public Guid? MemberId { get; set; }

        /// <summary>
        /// 成员类型
        /// </summary>
        public int MemberType { get; set; }

        /// <summary>
        /// 角色列表
        /// </summary>
        public List<RoleData> Roles { get; set; }

        /// <summary>
        /// 创建帐号数据
        /// </summary>
        public AccountData()
        {
            Roles = new List<RoleData>();
        }

        /// <summary>
        /// 是否有权限操作
        /// </summary>
        /// <param name="resourceId">资源序号</param>
        /// <param name="opration">操作</param>
        /// <returns></returns>
        public bool HasPower(Guid resourceId, Operation opration = Operation.List)
        {
            foreach (var role in Roles)
            {
                if (role.Permissions.FirstOrDefault(o => o.ResourceId == resourceId && (int)o.Operator == (int)opration) != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
