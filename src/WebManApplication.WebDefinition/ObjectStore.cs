using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebManApplication
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
            object result;
            if (Store.TryGetValue(type, out result)) return result;

            result = Activator.CreateInstance(type);
            Store.TryAdd(type, result);
            return result;
        }
    }
}
