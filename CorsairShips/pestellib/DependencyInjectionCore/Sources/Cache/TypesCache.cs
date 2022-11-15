using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityDI
{
    public class TypesCache
    {
        private readonly ConcurrentDictionary<Type, CachedType> _cache = new ConcurrentDictionary<Type, CachedType>();

        public CachedType GetFromCache(Type type)
        {
            if (_cache.ContainsKey(type)) return _cache[type];

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            FieldInfo[] fields = type.GetFields(flags);

            List<CachedDependency> deps = new List<CachedDependency>();

            foreach (FieldInfo field in fields)
            {
                var attrs = field.GetCustomAttributes(typeof(DependencyAttribute), true);
                if (!attrs.Any())
                    continue;

                var attrib = (DependencyAttribute)attrs[0];
                
                try
                {
                    deps.Add(new CachedDependency
                    {
                        Field = field,
                        Attribute = attrib
                    });
                }
                catch (ContainerException ex)
                {
                    throw new ContainerException("Can't resolve property \"" + field.Name + "\" of class \"" + type.FullName + "\"", ex);
                }
            }

            var result = new CachedType
            {
                Dependencies = deps.ToArray()
            };

            _cache.TryAdd(type, result);
            return result;
        }
    }
}