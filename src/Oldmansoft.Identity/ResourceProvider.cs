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

            var name = summary.GetSummary(type.GetFullName());
            if (string.IsNullOrWhiteSpace(name)) name = type.Name;
            var result = Domain.Resource.Create(CreateGuid(type, string.Empty), name);
            
            foreach (var property in type.GetProperties())
            {
                if (property.CanWrite && property.PropertyType == typeof(Guid))
                {
                    name = summary.GetSummary(property.Name);
                    if (string.IsNullOrWhiteSpace(name)) name = property.Name;
                    result.Add(Domain.ResourceItem.Create(CreateGuid(property.DeclaringType, property.Name), name));
                }
            }

            foreach (var field in type.GetFields())
            {
                if (field.IsPublic && field.FieldType == typeof(Guid))
                {
                    name = summary.GetSummary(field.Name);
                    if (name == null) name = field.Name;
                    result.Add(Domain.ResourceItem.Create(CreateGuid(field.DeclaringType, field.Name), name));
                }
            }
            return result;
        }
    }
}
