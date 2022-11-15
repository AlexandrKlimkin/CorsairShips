using System;

public class ChatRemoteClock
{
    public ChatRemoteClock(long remoteTicks)
    {
        _ticksDelta = DateTime.UtcNow.Ticks - remoteTicks;
    }

    public void UpdateDelta(long remoteTicks, long localPredicted)
    {
        var dt = DateTime.UtcNow - new DateTime(localPredicted);
        Latency = (DateTime.UtcNow - new DateTime(localPredicted)).Ticks;
        var delta = DateTime.UtcNow.Ticks - remoteTicks;
        _ticksDelta = (_ticksDelta + delta) / 2;
    }

    public long RemoteNow => DateTime.UtcNow.Ticks - _ticksDelta;
    public long LocalNow => DateTime.UtcNow.Ticks;
    public long Latency { get; private set; }

    private long _ticksDelta;
}
