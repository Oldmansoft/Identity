using Oldmansoft.ClassicDomain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

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

        private static ConcurrentDictionary<Type, List<Domain.Resource>> ListStore { get; set; }

        static ResourceProvider()
        {
            StructureStore = new ConcurrentDictionary<Type, object>();
            DomainStore = new ConcurrentDictionary<Type, Domain.Resource>();
            ListStore = new ConcurrentDictionary<Type, List<Domain.Resource>>();
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
            if (StructureStore.TryGetValue(type, out object value)) return value as TOperateResource;

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
            if (DomainStore.TryGetValue(type, out Domain.Resource value)) return value;

            var summary = Util.AssemblyXml.SummaryReader.GetTypeInfo(type);
            var result = Domain.Resource.Create(CreateGuid(type, string.Empty), GetName(type, summary, type.GetFullName(), type.Name));

            foreach (var property in type.GetProperties())
            {
                if (property.CanWrite && property.PropertyType == typeof(Guid))
                {
                    result.Add(Domain.ResourceItem.CreateItem(CreateGuid(property.DeclaringType, property.Name), GetName(property, summary, property.Name, property.Name)));
                }
            }

            foreach (var field in type.GetFields())
            {
                if (field.IsPublic && field.FieldType == typeof(Guid))
                {
                    result.Add(Domain.ResourceItem.CreateItem(CreateGuid(field.DeclaringType, field.Name), GetName(field, summary, field.Name, field.Name)));
                }
            }

            DomainStore.TryAdd(type, result);
            return result;
        }

        private static string GetName(MemberInfo info, Util.AssemblyXml.SummaryTypeInfo summary, string member, string name)
        {
            if (info.GetCustomAttribute(typeof(DescriptionAttribute), false) is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }
            var summaryName = summary.GetSummary(member);
            if (!string.IsNullOrWhiteSpace(summaryName))
            {
                return summaryName;
            }
            return name;
        }

        private static IList<Domain.Resource> GetResourceFromAssembly(Type typeOfResource)
        {
            if (ListStore.TryGetValue(typeOfResource, out List<Domain.Resource> value)) return value;

            var result = new List<Domain.Resource>();
            var types = Assembly.GetAssembly(typeOfResource).GetTypes();
            foreach(var type in types)
            {
                if (!type.GetInterfaces().Contains(typeof(IOperateResource))) continue;
                result.Add(GetResource(type));
            }

            ListStore.TryAdd(typeOfResource, result);
            return result;
        }

        /// <summary>
        /// 获取资源数据
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Data.ResourceData GetResourceData<TResource>(Guid id)
            where TResource : class
        {
            var list = GetResourceFromAssembly(typeof(TResource));
            var domain = list.FirstOrDefault(o => o.Id == id);
            return domain.MapTo(new Data.ResourceData());
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
