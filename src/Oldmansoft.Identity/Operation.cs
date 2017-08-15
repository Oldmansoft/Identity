using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [Description("列表")]
        List = 1,

        /// <summary>
        /// 执行
        /// </summary>
        [Description("执行")]
        Execute = 2,

        /// <summary>
        /// 查看
        /// </summary>
        [Description("查看")]
        View = 4,

        /// <summary>
        /// 添加
        /// </summary>
        [Description("添加")]
        Append = 8,

        /// <summary>
        /// 修改
        /// </summary>
        [Description("修改")]
        Modify = 16,

        /// <summary>
        /// 移除
        /// </summary>
        [Description("移除")]
        Remove = 32
    }
}