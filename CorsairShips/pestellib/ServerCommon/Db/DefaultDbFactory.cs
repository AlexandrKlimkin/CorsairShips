using System;
using MongoDB.Driver;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace PestelLib.ServerCommon.Db
{
    public static class DefaultDbFactory<T>
    {
        public static ConstructorInfo _ctor;

        static DefaultDbFactory()
        {
            var type = typeof(T);
            if (!type.IsInterface)
                throw new NotSupportedException($"{type.FullName} is not an interface.");
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = new List<Type>();
            for (var i = 0; i < assemblies.Length; ++i)
            {
                try
                {
                    types.AddRange(assemblies[i].GetTypes().Where(_ => type.IsAssignableFrom(_)));
                }
                catch
                { 
                }
            }

            foreach (var t in types)
            {
                _ctor = t.GetConstructor(new[] { typeof(MongoUrl) });
                if (_ctor != null)
                    return;
            }
            throw new NotImplementedException($"Interface {type.FullName} has no implementations which takes MongoUrl as a constructor parameter.");
        }

        public static T Create(string connectionString)
        {
            MongoUrl url = null;
            try
            {
                url = new MongoUrl(connectionString);
            }
            catch
            { }

            if (url != null)
            {
                return (T)_ctor.Invoke(new[] { url });
            }
            throw new NotImplementedException($"Can't create object of type {typeof(T).FullName} with connection string '{connectionString}'.");
        }
    }
}
