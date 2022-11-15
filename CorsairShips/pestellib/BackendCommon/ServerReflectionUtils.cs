using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace Server
{
    public static class ServerReflectionUtils
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ServerReflectionUtils));

        public static Type[] GetAllInterfaceImplementations(Type interfaceType)
        {
            var result = new List<Type>();
            Assembly assemblyDbg = null;
            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    assemblyDbg = assembly;
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!interfaceType.IsAssignableFrom(type) || type == interfaceType)
                            continue;
                        result.Add(type);
                    }
                }
            }
            catch (Exception e)
            {
                string msg;
                log.Error($"GetAllInterfaceImplementations({interfaceType.FullName}) assembly {assemblyDbg?.FullName}", e);
                if (e is ReflectionTypeLoadException reflectionTypeLoadException)
                {
                    msg = string.Join("\n", reflectionTypeLoadException.LoaderExceptions.Select(_ => _.ToString()));
                    log.Error(msg);
                }
                System.Diagnostics.Debugger.Break();
                throw;
            }

            return result.ToArray();
        }

        public static Type GetTheMostDerivedType(Assembly assembly, Type baseType)
        {
            var allModules = assembly
                .GetTypes()
                .Where(type =>
                    (baseType.IsAssignableFrom(type) || baseType == type)
                    && !type.IsInterface
                    && !type.IsAbstract)
                .OrderBy(x => x.Name)
                .ToList();

            //оставляем только те классы, у которых нет классов-наследников:
            var theMostDerivedTypes = allModules.Where(x => allModules.All(y => y.BaseType != x)).ToList();
            if (theMostDerivedTypes.Count > 1)
            {
                var typesList = string.Join(", ", theMostDerivedTypes);
                log.ErrorFormat("GetTheMostDerivedType(...) find multiple ({0}) derived classes from base type {1}", typesList, baseType.Name);
            }
            else if (theMostDerivedTypes.Count == 0)
            {
                log.WarnFormat("GetTheMostDerivedType(...) can't find any types for {0}. Using default class DefaultStateFactory", baseType.Name);
            }

            return theMostDerivedTypes.FirstOrDefault();
        }
    }
}