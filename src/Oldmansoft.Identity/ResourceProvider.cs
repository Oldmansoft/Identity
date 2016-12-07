using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 资源提供者
    /// </summary>
    public class ResourceProvider
    {
        /// <summary>
        /// 缓存
        /// </summary>
        private static Dictionary<Type, object> Store { get; set; }

        static ResourceProvider()
        {
            Store = new Dictionary<Type, object>();
        }
        
        private static T CreateResource<T>(Type type) where T : class, new()
        {
            T result = new T();
            
            foreach (var property in type.GetProperties())
            {
                if (property.CanWrite && property.PropertyType == typeof(Guid))
                {
                    property.SetValue(result, CreateGuid(property.DeclaringType, property.Name));
                }
            }
            foreach (var field in type.GetFields())
            {
                if (field.IsPublic && field.FieldType == typeof(Guid))
                {
                    field.SetValue(result, CreateGuid(field.DeclaringType, field.Name));
                }
            }
            return result;
        }

        private static Guid CreateGuid(Type type, string name)
        {
            var source = Encoding.ASCII.GetBytes(string.Format("IdentityResource:{0}.{1}", type.FullName, name)).GetSHA256Hash();
            var input = new byte[16];
            Array.Copy(source, input, 16);
            return new Guid(input);
        }


        /// <summary>
        /// 获取资源
        /// </summary>
        /// <typeparam name="TOperateResource"></typeparam>
        /// <returns></returns>
        public static TOperateResource Get<TOperateResource>()
            where TOperateResource : class, IOperateResource, new()
        {
            Type type = typeof(TOperateResource);
            if (!Store.ContainsKey(type))
            {
                lock (Store)
                {
                    if (!Store.ContainsKey(type))
                    {
                        Store.Add(type, CreateResource<TOperateResource>(type));
                    }
                }
            }
            return Store[type] as TOperateResource;
        }
        
        internal static Domain.Resource GetResource<TOperateResource>()
            where TOperateResource : class, IOperateResource, new()
        {
            var type = typeof(TOperateResource);
            var summary = Util.AssemblyXml.SummaryReader.GetTypeInfo(type);

            var result = new Domain.Resource();
            result.Id = CreateGuid(type, string.Empty);
            result.Name = summary.GetSummary(type.FullName);
            if (result.Name == null) result.Name = type.Name;

            foreach (var property in type.GetProperties())
            {
                if (property.CanWrite && property.PropertyType == typeof(Guid))
                {
                    var data = new Domain.ResourceItem();
                    data.Id = CreateGuid(property.DeclaringType, property.Name);
                    data.Name = summary.GetSummary(property.Name);
                    if (data.Name == null) data.Name = property.Name;
                    result.Add(data);
                }
            }

            foreach (var field in type.GetFields())
            {
                if (field.IsPublic && field.FieldType == typeof(Guid))
                {
                    var data = new Domain.ResourceItem();
                    data.Id = CreateGuid(field.DeclaringType, field.Name);
                    data.Name = summary.GetSummary(field.Name);
                    if (data.Name == null) data.Name = field.Name;
                    result.Add(data);
                }
            }
            return result;
        }
    }
}
