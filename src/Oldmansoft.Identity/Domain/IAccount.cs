using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Domain
{
    /// <summary>
    /// 帐号接口
    /// </summary>
    public interface IAccount
    {

        /// <summary>
        /// 是否包含角色序号
        /// </summary>
        /// <param name="roleId">角色序号</param>
        /// <returns></returns>
        bool ContainsRoleId(Guid roleId);
    }
}
