using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Data
{
    public class PermissionData
    {
        public Guid Id { get; set; }
        
        public Operation Operator { get; set; }

        public Guid ResourceId { get; set; }
    }
}
