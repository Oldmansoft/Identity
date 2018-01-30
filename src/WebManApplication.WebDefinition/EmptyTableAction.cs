using Oldmansoft.Html.WebMan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebManApplication
{
    class EmptyTableAction : ITableAction
    {
        public static readonly EmptyTableAction Instance = new EmptyTableAction();

        private EmptyTableAction()
        {
        }

        public ITableAction Confirm(string content)
        {
            return this;
        }

        public ITableAction NeedSelected()
        {
            return this;
        }

        public ITableAction SupportParameter()
        {
            return this;
        }
    }

    class EmptyTableAction<TModel> : IStaticTableItemAction<TModel>
    {
        public static readonly EmptyTableAction<TModel> Instance = new EmptyTableAction<TModel>();

        private EmptyTableAction()
        {
        }

        public IStaticTableItemAction<TModel> Confirm(string content)
        {
            return this;
        }

        public IStaticTableItemAction<TModel> OnClientCondition(ItemActionClient action, Func<TModel, bool> condition)
        {
            return this;
        }
    }
}
