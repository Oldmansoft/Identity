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
        public Guid Id { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 资源列表
        /// </summary>
        public List<ResourceItem> Children { get; private set; }

        private Resource()
        {
            Children = new List<ResourceItem>();
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Resource Create(Guid id, string name)
        {
            var result = new Resource();
            result.Id = id;
            result.Name = name;
            return result;
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