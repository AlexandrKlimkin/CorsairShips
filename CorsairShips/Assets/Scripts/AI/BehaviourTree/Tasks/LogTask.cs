using System;
using UnityEngine;

namespace Tools.BehaviourTree {


    public class LogTask : Task {

        public object Message;


        public override void Init() {            
        }

        public override void Begin() {
        }

        public override TaskStatus Run() {
            Debug.Log(Message);
            return TaskStatus.Success;
        }
    }
}
