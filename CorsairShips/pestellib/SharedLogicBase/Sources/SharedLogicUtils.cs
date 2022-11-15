using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using PestelLib.ServerShared;

namespace PestelLib.SharedLogicBase
{
    internal class SharedLogicUtils<TDef>
    {
        private static Dictionary<string, Type> _allModules;
        private static Dictionary<int, Type> _commandCodeToType;

        public static Dictionary<string, Type> AllModules
        {
            get
            {
                if (_allModules == null)
                {
                    _allModules = new Dictionary<string, Type>();
                    var moduleTypes = typeof(TDef).Assembly
                        .GetTypes()
                        .Where(type =>
                            typeof(ISharedLogicModule).IsAssignableFrom(type)
                            && !type.IsInterface
                            && !type.IsGenericType)
                        .ToList();

                    foreach (var module in moduleTypes)
                    {
                        _allModules.Add(module.Name, module);
                    }
                }
                return _allModules;
            }
        }

        public static Dictionary<int, Type> CommandCodeToType
        {
            get
            {
                if (_commandCodeToType == null)
                {
                    _commandCodeToType = new Dictionary<int, Type>();

                    var cmdTypes = typeof(TDef).Assembly
                        .GetTypes()
                        .Where(type =>
                            type.Namespace == "S"
                            && type.IsDefined(typeof(MessagePackObjectAttribute), false)
                            && !type.IsInterface
                            && !type.IsGenericType
                            && !type.IsAbstract)
                        .ToList();

                    foreach (var t in cmdTypes)
                    {
                        _commandCodeToType[CommandCode.CodeByName(t.Name)] = t;
                    }
                }

                return _commandCodeToType;
            }
        }
    }
}