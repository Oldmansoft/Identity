﻿using System;
using System.Collections.Generic;

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
        private Account()
        {
            CreatedTime = DateTime.UtcNow;
            RoleIds = new List<Guid>();
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="partitionResourceId"></param>
        /// <param name="name"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        public static Account Create(Guid partitionResourceId, string name, int memberType)
        {
            var result = new Account();
            result.PartitionResourceId = partitionResourceId;
            result.Name = name;
            result.MemberType = memberType;
            return result;
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
