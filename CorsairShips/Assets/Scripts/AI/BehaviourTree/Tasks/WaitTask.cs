using UnityEngine;

namespace Tools.BehaviourTree {

    public class WaitTask : Task {

        public float Delay;

        private float _CurrentDelay;
        private float _LastTimeStamp;
        private float _DeltaTime;

        public override void Init() {
        }

        public override void Begin() {
            _CurrentDelay = Delay;
            _LastTimeStamp = Time.time;
        }

        public override TaskStatus Run() {
            _DeltaTime = Time.time - _LastTimeStamp;
            _CurrentDelay -= _DeltaTime;
            _LastTimeStamp = Time.time;
            if (_CurrentDelay > 0) {
                return TaskStatus.Running;
            }
            return TaskStatus.Success;
        }
    }
}
