using Oldmansoft.Html.WebMan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebManApplication
{
    class EmptyItemAction : IItemAction
    {
        public static readonly EmptyItemAction Instance = new EmptyItemAction();

        private EmptyItemAction()
        {
        }

        public IItemAction Confirm(string content)
        {
            return this;
        }

        public IItemAction OnClientCondition(ItemActionClient action, string condition)
        {
            return this;
        }
    }
}
