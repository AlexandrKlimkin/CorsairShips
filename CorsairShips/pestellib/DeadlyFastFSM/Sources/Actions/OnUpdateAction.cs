using System;

namespace PestelLib.DeadlyFastFSM.Actions
{
    public class OnUpdateAction : IStateAction
    {
        private Action _onUpdate;

        public OnUpdateAction(Action onUpdate)
        {
            _onUpdate = onUpdate;
        }

        public Action OnFinish { get; set; }

        public void OnEnter() { }

        public void Update()
        {
            _onUpdate();
        }
    }
}
