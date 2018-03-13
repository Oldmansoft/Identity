﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Guid Id { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }

        private ResourceItem() { }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ResourceItem Create(Guid id, string name)
        {
            var result = new ResourceItem();
            result.Id = id;
            result.Name = name;
            return result;
        }
    }
}
