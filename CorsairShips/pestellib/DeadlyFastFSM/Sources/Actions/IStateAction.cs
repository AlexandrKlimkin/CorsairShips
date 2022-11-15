using System;

namespace PestelLib.DeadlyFastFSM.Actions
{
    public interface IStateAction
    {
        Action OnFinish { get; set; }
        void OnEnter();
        void Update();
    }
}