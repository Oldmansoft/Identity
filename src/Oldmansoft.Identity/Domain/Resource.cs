using System;
using System.Collections.Generic;

namespace Oldmansoft.Identity.Domain
{
    /// <summary>
    /// 操作资源
    /// </summary>
    public class Resource : ResourceItem
    {
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
            return new Resource
            {
                Id = id,
                Name = name
            };
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