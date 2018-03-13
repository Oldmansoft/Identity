using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity.Domain
{
    /// <summary>
    /// 授权
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// 资源序号
        /// </summary>
        public Guid ResourceId { get; private set; }

        /// <summary>
        /// 操作
        /// </summary>
        public Operation Operator { get; private set; }

        /// <summary>
        /// 是否可以操作资源
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public bool HasOperator(Resource resource, Operation operation)
        {
            return ResourceId == resource.Id && Operator == operation;
        }
    }
}
