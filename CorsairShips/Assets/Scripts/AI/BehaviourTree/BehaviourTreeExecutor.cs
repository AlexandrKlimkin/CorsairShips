using System;
using UnityDI;
using UnityEngine;

namespace Tools.BehaviourTree {

    public abstract class BehaviourTreeExecutor : MonoBehaviour {

        public BehaviourTree BehaviourTree { get; protected set; }

        protected virtual bool InitializeOnAwake => false; 
        
        protected virtual void Awake() {
            if(!InitializeOnAwake)
                return;
            Init();
        }

        protected abstract void Initialize();

        public void Init() {
            Initialize();
            BehaviourTree = BuildBehaviourTree();
            BehaviourTree.Blackboard = BuildBlackboard();
            BehaviourTree.Executor = this;
            BehaviourTree.Init();
        }
        
        protected abstract BehaviourTree BuildBehaviourTree();

        protected abstract Blackboard BuildBlackboard();

        public virtual void UpdateBT() {
            BehaviourTree.UpdateTask();
        }

        public void SetBehaviour(BehaviourTree newTree) {
            BehaviourTree = newTree;
            BehaviourTree.Blackboard = BuildBlackboard();
            BehaviourTree.Executor = this;
            BehaviourTree.Init();
        }

        protected void DisposeTasks() {
            BehaviourTree?.Tasks?.ForEach(_ => _.Dispose());
        }
        
        protected virtual void OnDestroy() {
            DisposeTasks();
        }
    }
}