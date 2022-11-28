using System;
using UnityEngine;

namespace Tools.BehaviourTree {

    /// <summary>
    /// Logical OR
    /// </summary>
    public class SelectorTask : Task {

        public override void Init() {
        }

        public override void Begin() {
        }

        /// <returns>
        /// Failure only if all tasks failed,
        /// else first not failed task status.
        /// </returns>
        public override TaskStatus Run() {
            foreach(var childTask in Children) {
                var childStatus = childTask.UpdateTask();
                switch (childStatus) {
                    case TaskStatus.Failure:
                        break;
                    case TaskStatus.Success:
                    case TaskStatus.Running:
                        return childStatus;
                }
            }
            return TaskStatus.Failure;
        }
    }
}
