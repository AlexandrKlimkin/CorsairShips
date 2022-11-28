using System;
using UnityEngine;

namespace Tools.BehaviourTree {

    /// <summary>
    /// Invert child TaskStatus
    /// </summary>
    public class InverterTask : Task {

        public override void Init() {
        }

        public override void Begin() {
        }

        /// <returns>
        /// Success if child Failure
        /// Failure if child Success
        /// Running if child Running
        /// </returns>
        public override TaskStatus Run() {
            if (Children.Count != 1)
                return TaskStatus.Failure;
            switch (Children[0].UpdateTask()) {
                case TaskStatus.Failure:
                    return TaskStatus.Success;
                case TaskStatus.Success:
                    return TaskStatus.Failure;
                default:
                    return TaskStatus.Running;
            }
        }
    }
}