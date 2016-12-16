using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Domain
{
    /// <summary>
    /// 操作资源
    /// </summary>
    public class Resource
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
        /// 资源列表
        /// </summary>
        public List<ResourceItem> Children { get; set; }

        /// <summary>
        /// 创建操作资源
        /// </summary>
        public Resource()
        {
            Children = new List<ResourceItem>();
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="item"></param>
        public void Add(ResourceItem item)
        {
            Children.Add(item);
        }
    }
}