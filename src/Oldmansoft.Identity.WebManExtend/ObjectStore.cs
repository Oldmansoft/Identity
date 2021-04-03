using System;
using System.Collections.Concurrent;

namespace Oldmansoft.Identity
{
    class ObjectStore
    {
        public static readonly ObjectStore Instance = new ObjectStore();

        private ConcurrentDictionary<Type, object> Store { get; set; }

        private ObjectStore()
        {
            Store = new ConcurrentDictionary<Type, object>();
        }

        public object Get(Type type)
        {
            if (Store.TryGetValue(type, out object result)) return result;

            result = Activator.CreateInstance(type);
            Store.TryAdd(type, result);
            return result;
        }
    }
}
