using MessagePack;
using PestelLib.ServerShared;
using UnityDI;
using System;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using PestelLib.UniversalSerializer;

namespace PestelLib.SharedLogicBase
{
    public interface ISharedLogicModule
    {
        byte[] SerializedState { get; set; }
        void MakeDefaultState();
        object RawState { get; }
        int MakeDefaultStatePriority { get; }
        void OnDependenciesResolved();
    }
    [System.Reflection.Obfuscation(ApplyToMembers=false)]
    public class SharedLogicModule<T> : ISharedLogicModule where T : class, new()
    {
        private ISharedLogic _sharedLogic;

        protected ISharedLogic SharedLogic
        {
            get
            {
                if (_sharedLogic == null)
                {
                    _sharedLogic = Container.Resolve<ISharedLogic>();
                }
                return _sharedLogic;
            }
        }

        private IScheduledActionCaller _scheduledActionCaller;

        protected IScheduledActionCaller ScheduledActionCaller
        {
            get
            {
                if (_scheduledActionCaller == null)
                {
                    _scheduledActionCaller = Container.Resolve<IScheduledActionCaller>();
                }
                return _scheduledActionCaller;
            }
        }


        private IOrderedScheduledActionCaller _orderedScheduledActionCaller;

        protected IOrderedScheduledActionCaller OrderedScheduledActionCaller {
            get {
                if (_orderedScheduledActionCaller == null) {
                    _orderedScheduledActionCaller = Container.Resolve<IOrderedScheduledActionCaller>();
                }
                return _orderedScheduledActionCaller;
            }
        }

        protected ISerializer Serializer
        {
            get { return SharedLogic.Serializer; }
        }

        [Conditional("UNITY_ASSERTIONS")]
        public void Log(string message)
        {
            SharedLogic.Log(message);
        }

        protected T State;

        public virtual object RawState { get { return CloneMessagePackObject(State); } }
        
        [System.Reflection.Obfuscation(ApplyToMembers=false)]
        public Container Container { get; set; }

        public virtual byte[] SerializedState
        {
            get { return Serializer.Serialize(State); }
            set { State = Serializer.Deserialize<T>(value); }
        }

        public virtual void MakeDefaultState()
        {
            State = new T();
        }

        public virtual void OnDependenciesResolved() {}

        public TT CloneMessagePackObject<TT>(TT obj)
        {
            return Serializer.Deserialize<TT>(Serializer.Serialize(obj));
        }

        /// UTC time
        public DateTime CommandTimestamp
        {
            get { return SharedLogic.CommandTimestamp; }
        }

        public virtual int MakeDefaultStatePriority
        {
            get { return 1; }
        }
    }
}