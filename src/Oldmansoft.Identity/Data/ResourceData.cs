using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Data
{
    /// <summary>
    /// 资源数据
    /// </summary>
    public class ResourceData
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
        /// 子资源
        /// </summary>
        public List<ResourceItemData> Children { get; set; }
    }

    /// <summary>
    /// 资源数据项
    /// </summary>
    public class ResourceItemData
    {
        /// <summary>
        /// 序号
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
}
