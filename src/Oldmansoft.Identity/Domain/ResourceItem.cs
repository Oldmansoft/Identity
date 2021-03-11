using System;

namespace Oldmansoft.Identity.Domain
{
    /// <summary>
    /// 资源项
    /// </summary>
    public class ResourceItem
    {
        /// <summary>
        /// 序号
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 创建
        /// </summary>
        protected ResourceItem() { }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ResourceItem CreateItem(Guid id, string name)
        {
            return new ResourceItem
            {
                Id = id,
                Name = name
            };
        }
    }
}
