using System;
using UnityDI;
using System.Threading.Tasks;
using log4net;
using UnityEngine;
using System.Threading;

public static class UnityTaskExtensions
{
    public static void Callback(this Task t, Action callback)
    {
        if (_current == null)
        {
            Log.Error("SynchronizationContext not found.");
            return;
        }
        t.ContinueWith(_ => _current.Post(o => ProcessTask(_, callback), null));
    }

    private static bool ProcessTask(Task t, Action callback)
    {
        if (!t.IsFaulted)
        {
            try
            {
                callback?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        var exeptions = t.Exception.Flatten().InnerExceptions;
        for (var i = 0; i < exeptions.Count; ++i)
        {
            Log.Error(exeptions[i]);
        }
        return false;
    }

    [RuntimeInitializeOnLoadMethod]
    private static void CaptureContext()
    {
        _current = SynchronizationContext.Current;
    }

    private static readonly ILog Log = LogManager.GetLogger(typeof(TaskExtensions));
    private static SynchronizationContext _current;
}
