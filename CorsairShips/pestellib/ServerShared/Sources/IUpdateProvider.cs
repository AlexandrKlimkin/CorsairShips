using System;

namespace ServerShared
{
    public interface IUpdateProvider
    {
        event Action OnUpdate;
    }
}
