using System;

namespace PestelLib.DeadlyFastFSM.Actions
{
    public class OnEnterAction : IStateAction
    {
        private readonly Action _onEnter;

        public OnEnterAction(Action onEnter)
        {
            _onEnter = onEnter;
        }

        public Action OnFinish { get; set; }

        public void OnEnter()
        {
            _onEnter();
        }

        public void Update() {}
    }
}
