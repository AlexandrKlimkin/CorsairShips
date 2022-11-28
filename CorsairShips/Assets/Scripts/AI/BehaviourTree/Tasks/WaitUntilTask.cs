using System;
using UnityEngine;

namespace Tools.BehaviourTree {

    public class WaitUntilTask : Task {

        public Func<bool> Condition;

        public override void Init() {
        }

        public override void Begin() {
        }

        public override TaskStatus Run() {
            if (Condition == null)
                return TaskStatus.Failure;
            if (Condition()) {
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }
    }
}
