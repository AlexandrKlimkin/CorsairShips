using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityDI;
using System.Linq;
using System.Runtime.Serialization;

namespace PestelLib.SharedLogicBase
{
    public class ModuleProvider : IProviderWrapper
    {
        private readonly Dictionary<string, byte[]> _datas;
        private readonly Type _type;
        private object _instance;

        public ModuleProvider(Type moduleType, Dictionary<string, byte[]> datas)
        {
            this._datas = datas;
            _type = moduleType;

#if UNITY_EDITOR
            if (!moduleType.GetInterfaces().Contains(typeof(ISharedLogicModule)))
            {
                throw new Exception("moduleType have to implement interface ISharedLogicModule");
            }
#endif
        }

        public object GetObject(Container container)
        {
            if (_instance != null) return _instance;

            ISharedLogicModule module = null;
            try
            {
                module = (ISharedLogicModule)FormatterServices.GetUninitializedObject(_type);
                _type.GetProperty("Container").SetValue(module, container, null);
                _type.GetConstructor(Type.EmptyTypes).Invoke(module, null);
            }
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            catch (Exception e)
            {

                UnityEngine.Debug.LogError(string.Format("Can't create module instance of type {0}, exception: {1}", _type, e));
            }
#else
            catch { }
#endif
            _instance = module;

            container.BuildUp(_type, module);
           
            return module;
        }
    }
}