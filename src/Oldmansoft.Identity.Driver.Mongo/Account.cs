using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Driver.Mongo
{
    /// <summary>
    /// 帐号
    /// </summary>
    class Account : Domain.Account
    {
        /// <summary>
        /// 密码哈希值
        /// </summary>
        public byte[] PasswordHash { get; set; }

        /// <summary>
        /// 角色序号
        /// </summary>
        public List<Guid> RoleIds { get; set; }

        /// <summary>
        /// 创建帐号
        /// </summary>
        public Account()
        {
            RoleIds = new List<Guid>();
        }

        protected override byte[] GetPasswordHash()
        {
            return PasswordHash;
        }

        protected override void SetPasswordHash(byte[] hash)
        {
            PasswordHash = hash;
        }

        public override List<Guid> GetRoleIds()
        {
            return RoleIds;
        }

        public override void SetRoleIds(Guid[] roleIds)
        {
            RoleIds.Clear();
            if (roleIds == null || roleIds.Length == 0) return;
            RoleIds.AddRange(roleIds);
        }
    }
}
