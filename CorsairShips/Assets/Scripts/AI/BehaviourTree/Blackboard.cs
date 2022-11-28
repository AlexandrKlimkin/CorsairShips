using System;
using System.Collections.Generic;

namespace Tools.BehaviourTree {
    [Serializable]
    public class Blackboard {
        private readonly Dictionary<Type, IBlackboardData> _Container = new Dictionary<Type, IBlackboardData>();

        public T Get<T>() where T : class, IBlackboardData, new() {
            return Get(typeof(T)) as T;
        }

        public IBlackboardData Get(Type type) {
            if (_Container.TryGetValue(type, out var result))
                return result;
            var instance = Activator.CreateInstance(type) as IBlackboardData;
            Set(type, instance);
            return instance;
        }

        public void Set<T>(T parameter) where T : IBlackboardData {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));
            Set(parameter.GetType(), parameter);
        }

        public void Set(Type type, IBlackboardData parameter) {
            if (_Container.ContainsKey(type)) {
                _Container[type] = parameter;
            }
            else {
                _Container.Add(type, parameter);
            }
        }
    }
}