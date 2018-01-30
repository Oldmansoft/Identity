using Oldmansoft.Html;
using Oldmansoft.Html.WebMan;
using Oldmansoft.Identity;
using Oldmansoft.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebManApplication
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class Extends
    {
        /// <summary>
        /// 根据权限添加菜单
        /// </summary>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public static TreeListItem Add(this TreeListItem source, TreeListItem item, AccountData account)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (item == null) throw new ArgumentNullException("item");
            if (account == null) throw new ArgumentNullException("account");
            if (item.Location.Method == null) throw new ArgumentNullException("location", "Method 不能为空");
            if (item.Location.TargetType == null) throw new ArgumentNullException("location", "TargetType 不能为空");
            if (!item.Location.TargetType.IsSubclassOf(typeof(AuthController))) throw new ArgumentNullException("location", "必须来自 AuthController");

            var attribute = item.Location.Method.GetCustomAttribute<AuthAttribute>();
            if (attribute == null)
            {
                source.Add(item);
                return source;
            }
            var controller = ObjectStore.Instance.Get(item.Location.TargetType) as AuthController;
            var resourceId = controller.OperateResource;
            var operation = attribute.Operation;

            if (account.HasPower(resourceId, operation))
            {
                source.Add(item);
            }
            return source;
        }

        /// <summary>
        /// 根据权限添加动作
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="source"></param>
        /// <param name="location"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public static ITableAction AddActionTable<TModel>(this DataTableDefinition<TModel> source, ILocation location, AccountData account) where TModel : class
        {
            if (source == null) throw new ArgumentNullException("source");
            if (location == null) throw new ArgumentNullException("location");
            if (account == null) throw new ArgumentNullException("account");
            if (location.Method == null) throw new ArgumentNullException("location", "Method 不能为空");
            if (location.TargetType == null) throw new ArgumentNullException("location", "TargetType 不能为空");
            if (!location.TargetType.IsSubclassOf(typeof(AuthController))) throw new ArgumentNullException("location", "必须来自 AuthController");

            var attribute = location.Method.GetCustomAttribute<AuthAttribute>();
            if (attribute == null)
            {
                return source.AddActionTable(location);
            }
            var controller = ObjectStore.Instance.Get(location.TargetType) as AuthController;
            var resourceId = controller.OperateResource;
            var operation = attribute.Operation;

            if (account.HasPower(resourceId, operation))
            {
                return source.AddActionTable(location);
            }
            return EmptyTableAction.Instance;
        }

        /// <summary>
        /// 根据权限添加动作
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="source"></param>
        /// <param name="location"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public static IDataTableItemAction AddActionItem<TModel>(this DataTableDefinition<TModel> source, ILocation location, AccountData account) where TModel : class
        {
            if (source == null) throw new ArgumentNullException("source");
            if (location == null) throw new ArgumentNullException("location");
            if (account == null) throw new ArgumentNullException("account");
            if (location.Method == null) throw new ArgumentNullException("location", "Method 不能为空");
            if (location.TargetType == null) throw new ArgumentNullException("location", "TargetType 不能为空");
            if (!location.TargetType.IsSubclassOf(typeof(AuthController))) throw new ArgumentNullException("location", "必须来自 AuthController");

            var attribute = location.Method.GetCustomAttribute<AuthAttribute>();
            if (attribute == null)
            {
                return source.AddActionItem(location);
            }
            var controller = ObjectStore.Instance.Get(location.TargetType) as AuthController;
            var resourceId = controller.OperateResource;
            var operation = attribute.Operation;

            if (account.HasPower(resourceId, operation))
            {
                return source.AddActionItem(location);
            }
            return EmptyItemAction.Instance;
        }

        /// <summary>
        /// 根据权限添加动作
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="source"></param>
        /// <param name="location"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public static ITableAction AddActionTable<TModel>(this StaticTable<TModel> source, ILocation location, AccountData account) where TModel : class
        {
            if (source == null) throw new ArgumentNullException("source");
            if (location == null) throw new ArgumentNullException("location");
            if (account == null) throw new ArgumentNullException("account");
            if (location.Method == null) throw new ArgumentNullException("location", "Method 不能为空");
            if (location.TargetType == null) throw new ArgumentNullException("location", "TargetType 不能为空");
            if (!location.TargetType.IsSubclassOf(typeof(AuthController))) throw new ArgumentNullException("location", "必须来自 AuthController");

            var attribute = location.Method.GetCustomAttribute<AuthAttribute>();
            if (attribute == null)
            {
                return source.AddActionTable(location);
            }
            var controller = ObjectStore.Instance.Get(location.TargetType) as AuthController;
            var resourceId = controller.OperateResource;
            var operation = attribute.Operation;

            if (account.HasPower(resourceId, operation))
            {
                return source.AddActionTable(location);
            }
            return EmptyTableAction.Instance;
        }

        /// <summary>
        /// 根据权限添加动作
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="source"></param>
        /// <param name="location"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public static IStaticTableItemAction<TModel> AddActionItem<TModel>(this StaticTable<TModel> source, ILocation location, AccountData account) where TModel : class
        {
            if (source == null) throw new ArgumentNullException("source");
            if (location == null) throw new ArgumentNullException("location");
            if (account == null) throw new ArgumentNullException("account");
            if (location.Method == null) throw new ArgumentNullException("location", "Method 不能为空");
            if (location.TargetType == null) throw new ArgumentNullException("location", "TargetType 不能为空");
            if (!location.TargetType.IsSubclassOf(typeof(AuthController))) throw new ArgumentNullException("location", "必须来自 AuthController");

            var attribute = location.Method.GetCustomAttribute<AuthAttribute>();
            if (attribute == null)
            {
                return source.AddActionItem(location);
            }
            var controller = ObjectStore.Instance.Get(location.TargetType) as AuthController;
            var resourceId = controller.OperateResource;
            var operation = attribute.Operation;

            if (account.HasPower(resourceId, operation))
            {
                return source.AddActionItem(location);
            }
            return EmptyTableAction<TModel>.Instance;
        }

        /// <summary>
        /// 添加查找面板
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="source"></param>
        /// <param name="location"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="placeHolder"></param>
        public static void AddSearchPanel<TModel>(this DataTableDefinition<TModel> source, ILocation location, string key, string value, string placeHolder = null) where TModel : class
        {
            if (source == null) return;
            if (location == null) throw new ArgumentNullException("location");
            if (key == null) throw new ArgumentNullException("key");

            var connector = "?";
            if (location.Path.IndexOf("?") > -1) connector = "&";
            var script = string.Format("$app.sameHash('{0}{1}{2}=' + encodeURIComponent($.trim($(this).find('input[name={2}]').val()))); return false;", location.Path, connector, key);

            var form = new HtmlElement(HtmlTag.Form);
            form.AddClass("bv-form");
            form.OnClient(HtmlEvent.Submit, script);
            form.PrependTo(source);

            var search = new HtmlElement(HtmlTag.Div);
            search.AddClass("form-group");
            search.AppendTo(form);

            var group = new HtmlElement(HtmlTag.Div);
            group.AppendTo(search);
            group.AddClass(Column.Sm5);
            group.AddClass("input-group");
            var input = new HtmlElement(HtmlTag.Input);
            input.AppendTo(group);
            input.Attribute(HtmlAttribute.Name, key);
            input.Attribute(HtmlAttribute.Value, value);
            input.Attribute(HtmlAttribute.PlaceHolder, placeHolder);
            input.AddClass("form-control");
            var span = new HtmlElement(HtmlTag.Span);
            span.AddClass("input-group-addon");
            span.AppendTo(group);
            var button = new HtmlElement(HtmlTag.Input);
            button.AppendTo(span);
            button.Attribute(HtmlAttribute.Value, "查找");
            button.Attribute(HtmlAttribute.Type, "submit");
        }

        /// <summary>
        /// 获以操作资源的数据源列表
        /// </summary>
        /// <returns></returns>
        public static IList<ListDataItem> GetOperationDataSourceList()
        {
            var list = new List<ListDataItem>();
            var enumType = typeof(Operation);
            foreach (var item in Enum.GetValues(enumType))
            {
                var name = Enum.GetName(typeof(Operation), item);
                var attribute = enumType.GetMember(name)[0].GetCustomAttribute(typeof(DescriptionAttribute), false) as DescriptionAttribute;
                if (attribute != null)
                {
                    name = attribute.Description;
                }
                list.Add(new ListDataItem(name, ((int)item).ToString()));
            }

            return list;
        }
    }
}
