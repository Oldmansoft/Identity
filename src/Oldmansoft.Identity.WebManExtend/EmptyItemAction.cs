using Oldmansoft.Html.WebMan;

namespace Oldmansoft.Identity
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
