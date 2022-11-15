using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using MessagePack;
using Newtonsoft.Json;
using PestelLib.SharedLogicBase;
using UnityDI;

namespace PestelLib.SharedLogic.Modules
{
    [System.Reflection.Obfuscation(ApplyToMembers=false)]
    public class CompositeModule<TModule> : SharedLogicModule<CompositeModuleState>, IDependent
        where TModule : ISharedLogicModule
    {
        public SortedDictionary<string, TModule> Modules = new SortedDictionary<string, TModule>();

        public virtual string[] GetModuleNames()
        {
            return new string[0];
        }

        protected virtual void OnInitialize()
        {

        }

        protected virtual Container GetDependencyInjectionContainer(string moduleName)
        {
            var container = new Container
            {
                ParentContainer = Container
            };

            CustomizeDependencyInjectionContainer(moduleName, container);

            return container;
        }

        protected virtual void CustomizeDependencyInjectionContainer(string moduleName, Container container) { }

        public override void MakeDefaultState()
        {
            base.MakeDefaultState();
            foreach (var moduleName in Modules.Keys)
            {
                Modules[moduleName].MakeDefaultState();
            }
        }

        public override byte[] SerializedState
        {
            set
            {
                base.SerializedState = value;

                foreach (var moduleName in State.ModulesDict.Keys)
                {
                    if (!State.ModulesDict.ContainsKey(moduleName))
                    {
                        Modules[moduleName].MakeDefaultState();
                    }
                    else
                    {
                        Modules[moduleName].SerializedState = State.ModulesDict[moduleName];
                    }
                }
            }

            get
            {
                foreach (var kv in Modules)
                {
                    State.ModulesDict[kv.Key] = kv.Value.SerializedState;
                }
                return base.SerializedState;
            }
        }

        public void OnInjected()
        {
            foreach (var moduleName in GetModuleNames())
            {
                Modules[moduleName] = (TModule)InstantiateSharedLogicModule(GetDependencyInjectionContainer(moduleName), typeof(TModule), true);
            }
        }

        public static ISharedLogicModule InstantiateSharedLogicModule(Container container, Type t, bool instantBuildUp = false)
        {
            ISharedLogicModule module = null;
            try
            {
                module = (ISharedLogicModule)FormatterServices.GetUninitializedObject(t);
                t.GetProperty("Container").SetValue(module, container, null);
                t.GetConstructor(Type.EmptyTypes).Invoke(module, null);
            }
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            catch (Exception e)
            {

                UnityEngine.Debug.LogError(string.Format("Can't create module instance of type {0}, exception: {1}", t, e));
            }
#else
			catch
            { }
#endif
            

            if (instantBuildUp)
            {
                container.BuildUp(t, module);
            }

            return module;
        }
    }
}