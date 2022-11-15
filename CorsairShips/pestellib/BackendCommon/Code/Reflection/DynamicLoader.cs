using System;
using System.Linq;
using System.IO;
using System.Reflection;
using BackendCommon.Code;
using System.Collections.Generic;
using log4net;
using ServerExtension;

namespace Backend.Code.Reflection
{
    public static class DynamicLoader
    {
        public static T CreateInstance<T>(string fullyQualifiedTypeName)
        {
            if (string.IsNullOrEmpty(fullyQualifiedTypeName))
                return default(T);
            var parts = fullyQualifiedTypeName.Split(',');
            if (parts.Length != 2)
                return default(T);
            var assemblyName = new AssemblyName(parts[1].Trim());
            var assembly = LoadAssembly(assemblyName);
            var type = assembly.GetType(parts[0]);
            var instance = Activator.CreateInstance(type);
            return (T) instance;
        }

        public static Assembly LoadAssembly(AssemblyName name)
        {
            var path = RuntimeSettings.AppRoot;
            string dllPath;
            Assembly assembly;
            try
            {
                assembly = AppDomain.CurrentDomain.Load(name);
            }
            catch
            {
                assembly = null;
            }

            if (assembly == null)
            {
                try
                {
                    dllPath = Path.Combine(path, "App_Data", $"{name}.dll");
                    assembly = Assembly.LoadFrom(dllPath);
                }
                catch
                {
                    assembly = null;
                }
            }

            if (assembly == null)
            {
                dllPath = Path.Combine(path, $"{name}.dll");
                assembly = Assembly.LoadFrom(dllPath);
            }

            return assembly;
        }

        public static List<Assembly> LoadByPattern(string pattern)
        {
            var result = new List<Assembly>();
            var path = RuntimeSettings.AppRoot;
            path = Path.Combine(path, "App_Data");
            var extraDlls = Directory.EnumerateFiles(path, pattern).ToArray();
            foreach (var dll in extraDlls)
            {
                result.Add(Assembly.LoadFrom(dll));
            }
            return result;
        }

        public static void LoadServerExtensions()
        {
            var r = LoadByPattern("Dep*.dll");
            var interfaceType = typeof(IServerExtensionInitializer);
            Assembly assembly = null;
            Type typeDbg = null;
            for (var i = 0; i < r.Count(); ++i)
            {
                try
                {
                    assembly = r[i];
                    foreach (var type in assembly.GetTypes())
                    {
                        typeDbg = type;
                        if (!interfaceType.IsAssignableFrom(type) || type == interfaceType)
                            continue;
                        var ctor = type.GetConstructor(new Type[] { });
                        var inst = ctor.Invoke(new object[] { }) as IServerExtensionInitializer;
                        inst.Init();
                        Log.Debug($"Extension initialized: {type.Name}.");
                    }
                }
                catch (Exception e)
                {
                    string msg;
                    Log.Error($"Loading IServerExtensionInitializer from {assembly?.FullName} last type {typeDbg?.FullName}.", e);
                    if (e is ReflectionTypeLoadException reflectionTypeLoadException)
                    {
                        msg = string.Join("\n", reflectionTypeLoadException.LoaderExceptions.Select(_ => _.ToString()));
                        Log.Error(msg);
                    }
                    System.Diagnostics.Debugger.Break();
                    throw;
                }
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(DynamicLoader));
    }
}