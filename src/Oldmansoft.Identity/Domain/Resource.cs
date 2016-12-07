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
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<ResourceItem> Children { get; set; }

        public Resource()
        {
            Children = new List<ResourceItem>();
        }

        public void Add(ResourceItem item)
        {
            Children.Add(item);
        }
    }
}