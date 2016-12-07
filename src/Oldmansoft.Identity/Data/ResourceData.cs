using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Data
{
    public class ResourceData
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public List<ResourceItemData> Children { get; set; }

        public class ResourceItemData
        {
            public Guid Id { get; set; }

            public string Name { get; set; }
        }
    }
}
