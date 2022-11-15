using System;

namespace PestelLib.Utils
{
    public interface ITimeProvider
    {
        bool IsSynced { get; }
        DateTime Now { get; }
    }
}