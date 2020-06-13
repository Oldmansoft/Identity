using Oldmansoft.Html.WebMan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebManApplication
{
    class EmptyItemAction : IDynamicTableItemAction
    {
        public static readonly EmptyItemAction Instance = new EmptyItemAction();

        private EmptyItemAction()
        {
        }

        public IDynamicTableItemAction Confirm(string content)
        {
            return this;
        }

        public IDynamicTableItemAction OnClientCondition(ItemActionClient action, string condition)
        {
            return this;
        }
    }
}
