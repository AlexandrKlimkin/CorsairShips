using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.BehaviourTree {


    public abstract class BehaviourTreeBuilder {
        public abstract BehaviourTree Build();
    }

    public abstract class BehaviourTreeBuilder<T>: BehaviourTreeBuilder where T : BehaviourTreeBuilder, new() {

        public static T Instance {
            get {
                if (_Instance == null)
                    _Instance = new T();
                return _Instance;
            }
        }
        private static T _Instance;
    }
}
