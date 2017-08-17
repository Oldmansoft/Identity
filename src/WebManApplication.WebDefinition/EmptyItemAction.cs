using Oldmansoft.Html.WebMan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebManApplication
{
    class EmptyItemAction : IDataTableItemAction
    {
        public static readonly EmptyItemAction Instance = new EmptyItemAction();

        private EmptyItemAction()
        {
        }

        public IDataTableItemAction Confirm(string content)
        {
            return this;
        }

        public IDataTableItemAction OnClientCondition(ItemActionClient action, string condition)
        {
            return this;
        }
    }
}
