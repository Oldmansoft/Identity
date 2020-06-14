using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 资源提供者
    /// </summary>
    public class ResourceProvider
    {
        /// <summary>
        /// 结构缓存
        /// </summary>
        private static ConcurrentDictionary<Type, object> StructureStore { get; set; }

        private static ConcurrentDictionary<Type, Domain.Resource> DomainStore { get; set; }

        static ResourceProvider()
        {
            StructureStore = new ConcurrentDictionary<Type, object>();
            DomainStore = new ConcurrentDictionary<Type, Domain.Resource>();
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
            object value;
            if (StructureStore.TryGetValue(type, out value)) return value as TOperateResource;

            var resource = CreateResource<TOperateResource>(type);
            StructureStore.TryAdd(type, resource);
            return resource;
        }

        internal static Domain.Resource GetResource<TOperateResource>()
            where TOperateResource : class, IOperateResource, new()
        {
            var type = typeof(TOperateResource);
            return GetResource(type);
        }

        private static Domain.Resource GetResource(Type type)
        {
            Domain.Resource value;
            if (DomainStore.TryGetValue(type, out value)) return value;

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
                    result.Add(Domain.ResourceItem.CreateItem(CreateGuid(property.DeclaringType, property.Name), name));
                }
            }

            foreach (var field in type.GetFields())
            {
                if (field.IsPublic && field.FieldType == typeof(Guid))
                {
                    name = summary.GetSummary(field.Name);
                    if (name == null) name = field.Name;
                    result.Add(Domain.ResourceItem.CreateItem(CreateGuid(field.DeclaringType, field.Name), name));
                }
            }

            DomainStore.TryAdd(type, result);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeOfAssembly"></param>
        /// <returns></returns>
        public static IList<Domain.Resource> GetResourceFromAssembly(Type typeOfAssembly)
        {
            var result = new List<Domain.Resource>();
            var types = System.Reflection.Assembly.GetAssembly(typeOfAssembly).GetTypes();
            foreach(var type in types)
            {
                if (!type.GetInterfaces().Contains(typeof(IOperateResource))) continue;
                result.Add(GetResource(type));
            }
            return result;
        }

        /// <summary>
        /// 获取资源数据
        /// </summary>
        /// <typeparam name="TOperateResource"></typeparam>
        /// <returns></returns>
        public static Data.ResourceData GetResourceData<TOperateResource>()
            where TOperateResource : class, IOperateResource, new()
        {
            var domain = GetResource<TOperateResource>();
            return domain.MapTo(new Data.ResourceData());
        }
    }
}
