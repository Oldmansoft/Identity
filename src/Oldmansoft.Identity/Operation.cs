using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 操作
    /// </summary>
    [Flags]
    public enum Operation
    {
        /// <summary>
        /// 列表
        /// </summary>
        List = 1,

        /// <summary>
        /// 执行
        /// </summary>
        Execute = 2,

        /// <summary>
        /// 查看
        /// </summary>
        View = 4,

        /// <summary>
        /// 添加
        /// </summary>
        Append = 8,

        /// <summary>
        /// 修改
        /// </summary>
        Modify = 16,

        /// <summary>
        /// 移除
        /// </summary>
        Remove = 32
    }
}