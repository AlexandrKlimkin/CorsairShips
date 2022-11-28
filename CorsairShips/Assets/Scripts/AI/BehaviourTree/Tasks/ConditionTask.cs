using System;
using UnityEngine;

namespace Tools.BehaviourTree {

    public class ConditionTask : Task {

        public Func<bool> Condition;
        public TaskStatus OnConditionFailStatus = TaskStatus.Failure;

        public override void Init() {
        }

        public override void Begin() {
        }

        public override TaskStatus Run() {
            if (Condition == null)
                return TaskStatus.Failure;
            if (Children.Count != 1) {
                throw new NotSupportedException($"{GetType().Name}: Condition Task with multiple children not supported");
            }
            if (Condition()) {
                return Children[0].UpdateTask();
            }
            return OnConditionFailStatus;
        }
    }
}
