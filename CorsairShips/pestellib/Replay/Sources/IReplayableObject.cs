using System;

namespace PestelLib.Replay
{
    public interface IReplayableObject
    {
        event Action OnComplete;
        bool IsCompleted { get; }
        void Replay(float duration);
        void Skip();
    }
}