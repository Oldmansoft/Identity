using Oldmansoft.Html.WebMan;
using Oldmansoft.Identity.Data;
using Oldmansoft.Identity.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class Extends
    {
        private static IAuthAttribute GetAuthAttribute(MethodInfo method)
        {
            foreach (var attribute in method.GetCustomAttributes())
            {
                if (attribute is IAuthAttribute)
                {
                    return attribute as IAuthAttribute;
                }
            }
            return null;
        }

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
            if (item == null) return source;
            if (account == null) return source;
            if (item.Location.Method == null) throw new ArgumentNullException("location", "Method 不能为空");
            if (item.Location.TargetType == null) throw new ArgumentNullException("location", "TargetType 不能为空");
            if (!item.Location.TargetType.GetInterfaces().Contains(typeof(IAuthController))) throw new ArgumentNullException("location", "必须来自 AuthController");

            var attribute = GetAuthAttribute(item.Location.Method);
            if (attribute == null)
            {
                source.Add(item);
                return source;
            }
            var controller = ObjectStore.Instance.Get(item.Location.TargetType) as IAuthController;
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
        public static ITableAction AddActionTable<TModel>(this DynamicTable<TModel> source, ILocation location, AccountData account) where TModel : class
        {
            if (source == null) throw new ArgumentNullException("source");
            if (location == null) throw new ArgumentNullException("location");
            if (account == null) return EmptyTableAction.Instance;
            if (location.Method == null) throw new ArgumentNullException("location", "Method 不能为空");
            if (location.TargetType == null) throw new ArgumentNullException("location", "TargetType 不能为空");
            if (!location.TargetType.GetInterfaces().Contains(typeof(IAuthController))) throw new ArgumentNullException("location", "必须来自 AuthController");

            var attribute = GetAuthAttribute(location.Method);
            if (attribute == null)
            {
                return source.AddActionTable(location);
            }
            var controller = ObjectStore.Instance.Get(location.TargetType) as IAuthController;
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
        public static IDynamicTableItemAction AddActionItem<TModel>(this DynamicTable<TModel> source, ILocation location, AccountData account) where TModel : class
        {
            if (source == null) throw new ArgumentNullException("source");
            if (location == null) throw new ArgumentNullException("location");
            if (account == null) return EmptyItemAction.Instance;
            if (location.Method == null) throw new ArgumentNullException("location", "Method 不能为空");
            if (location.TargetType == null) throw new ArgumentNullException("location", "TargetType 不能为空");
            if (!location.TargetType.GetInterfaces().Contains(typeof(IAuthController))) throw new ArgumentNullException("location", "必须来自 AuthController");

            var attribute = GetAuthAttribute(location.Method);
            if (attribute == null)
            {
                return source.AddActionItem(location);
            }
            var controller = ObjectStore.Instance.Get(location.TargetType) as IAuthController;
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
            if (account == null) return EmptyTableAction.Instance;
            if (location.Method == null) throw new ArgumentNullException("location", "Method 不能为空");
            if (location.TargetType == null) throw new ArgumentNullException("location", "TargetType 不能为空");
            if (!location.TargetType.GetInterfaces().Contains(typeof(IAuthController))) throw new ArgumentNullException("location", "必须来自 AuthController");

            var attribute = GetAuthAttribute(location.Method);
            if (attribute == null)
            {
                return source.AddActionTable(location);
            }
            var controller = ObjectStore.Instance.Get(location.TargetType) as IAuthController;
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
            if (account == null) return EmptyTableAction<TModel>.Instance;
            if (location.Method == null) throw new ArgumentNullException("location", "Method 不能为空");
            if (location.TargetType == null) throw new ArgumentNullException("location", "TargetType 不能为空");
            if (!location.TargetType.GetInterfaces().Contains(typeof(IAuthController))) throw new ArgumentNullException("location", "必须来自 AuthController");

            var attribute = GetAuthAttribute(location.Method);
            if (attribute == null)
            {
                return source.AddActionItem(location);
            }
            var controller = ObjectStore.Instance.Get(location.TargetType) as IAuthController;
            var resourceId = controller.OperateResource;
            var operation = attribute.Operation;

            if (account.HasPower(resourceId, operation))
            {
                return source.AddActionItem(location);
            }
            return EmptyTableAction<TModel>.Instance;
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
                if (enumType.GetMember(name)[0].GetCustomAttribute(typeof(DescriptionAttribute), false) is DescriptionAttribute attribute)
                {
                    name = attribute.Description;
                }
                list.Add(new ListDataItem(name, ((int)item).ToString()));
            }

            return list;
        }
    }
}
