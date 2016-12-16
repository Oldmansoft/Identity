﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 仓储工厂
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// 获取工作单元
        /// </summary>
        /// <returns></returns>
        IUnitOfWork GetUnitOfWork();

        /// <summary>
        /// 创建帐号对象
        /// </summary>
        /// <returns></returns>
        Domain.Account CreateAccountObject();

        /// <summary>
        /// 创建角色对象
        /// </summary>
        /// <returns></returns>
        Domain.Role CreateRoleObject();

        /// <summary>
        /// 创建帐号仓储
        /// </summary>
        /// <returns></returns>
        IRepository<Domain.Account, Guid> CreateAccountRepository();

        /// <summary>
        /// 创建角色仓储
        /// </summary>
        /// <returns></returns>
        IRepository<Domain.Role, Guid> CreateRoleRepository();
    }
}
