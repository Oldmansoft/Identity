using Oldmansoft.Identity;
using System;

namespace WebManApplicationCore
{
    /// <summary>
    /// 操作资源
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// 系统
        /// </summary>
        public class System : IOperateResource
        {
            /// <summary>
            /// 帐号管理
            /// </summary>
            public Guid Account { get; set; }

            /// <summary>
            /// 系统角色
            /// </summary>
            public Guid Role { get; set; }
        }

        /// <summary>
        /// 业务
        /// </summary>
        public class Business : IOperateResource
        {
            /// <summary>
            /// 帐号管理
            /// </summary>
            public Guid Account { get; set; }

            /// <summary>
            /// 业务角色
            /// </summary>
            public Guid Role { get; set; }
        }
    }
}
