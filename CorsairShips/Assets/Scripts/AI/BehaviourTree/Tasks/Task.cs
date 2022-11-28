using System.Collections.Generic;
using UnityEngine;

namespace Tools.BehaviourTree {

    public abstract class Task {

        protected Task() {
            Children = new List<Task>();
        }

        public BehaviourTree BehaviourTree { get; protected set; }
        public Task Parent { get; private set; }
        public TaskStatus Status { get; private set; }

        protected List<Task> Children { get; set; }

        private bool _Initialized = false;

        public TaskStatus UpdateTask() {
            if (!_Initialized) {
                Init();
                _Initialized = true;
            }

            //Debug.Log(this.GetType().Name + " Status: " + Status);
            if (Status != TaskStatus.Running) {
                //Debug.Log("Call Begin " + this.GetType().Name);
                Begin();
            }
            Status = Run();

            if (Parent != null) {
                switch (Status) {
                    case TaskStatus.Running: Parent.OnChildRunning(this); break;
                    case TaskStatus.Success: Parent.OnChildSuccess(this); break;
                    case TaskStatus.Failure: Parent.OnChildFailure(this); break;
                }
            }

            return Status;
        }

        public void Reset()
        {
            _Initialized = false;
            Status = TaskStatus.Failure;
        }
        
        public Task AddChild(Task child) {
            Children.Add(child);
            child.BehaviourTree = BehaviourTree;
            BehaviourTree.RegisterTask(child);
            OnChildAdded(child);
            return child;
        }

        public T AddChild<T>() where T : Task, new() {
            return AddChild(new T()).Cast<T>();
        }

        public T Cast<T>() where T : Task {
            return this as T;
        }

        public abstract void Init();
        public abstract void Begin();
        public abstract TaskStatus Run();

        public virtual void OnChildRunning(Task child) { }
        public virtual void OnChildSuccess(Task child) { }
        public virtual void OnChildFailure(Task child) { }
        public virtual void OnChildAdded(Task child) { }
        public virtual void Dispose() { }
    }
}
