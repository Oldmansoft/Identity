using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Data
{
    /// <summary>
    /// 授权数据
    /// </summary>
    public class PermissionData
    {
        /// <summary>
        /// 资源
        /// </summary>
        public Guid ResourceId { get; set; }

        /// <summary>
        /// 操作
        /// </summary>
        public Operation Operator { get; set; }
    }
}
