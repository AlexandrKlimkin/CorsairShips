using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PestelLib.NetworkUtils;
using UnityDI;
using UnityEngine;

public class InternetReachabilityDI : InternetReachabilityVerifier, IInternetReachability
{
    static readonly Thread mainThread = Thread.CurrentThread;
    private ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
    public bool HasInternet {
        get { return status == Status.NetVerified; }
    }

    protected override void Start()
    {
        ContainerHolder.Container.RegisterInstance<IInternetReachability>(this);
        base.Start();
    }

    public bool WaitInternet(int timeout)
    {
        if (mainThread == Thread.CurrentThread)
        {
            Debug.LogError("Can't wait on main thread");
            return false;
        }

        return _manualResetEvent.WaitOne(timeout);
    }

    void Update()
    {
        if (status == Status.NetVerified)
            _manualResetEvent.Set();
        else
            _manualResetEvent.Reset();
    }
}
