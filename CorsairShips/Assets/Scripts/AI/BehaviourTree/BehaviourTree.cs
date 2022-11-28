using System.Collections.Generic;
using UnityEngine;

namespace Tools.BehaviourTree {

    public class BehaviourTree : Task {

        public List<Task> Tasks { get; private set; }
        public Blackboard Blackboard { get; set; }
        public BehaviourTreeExecutor Executor { get; set; }

        public float DeltaTime { get; private set; }
        private float _LastRunTime;

        public BehaviourTree() : base() {
            BehaviourTree = this;
            Tasks = new List<Task>();
        }

        public override void Init() {
        }

        public override void Begin() {
        }

        public override TaskStatus Run() {
            DeltaTime = Time.time - _LastRunTime;
            _LastRunTime = Time.time;
            return Children[0].UpdateTask();
        }

        public void RegisterTask(Task task) {
            Tasks.Add(task);
        }
    }
}
