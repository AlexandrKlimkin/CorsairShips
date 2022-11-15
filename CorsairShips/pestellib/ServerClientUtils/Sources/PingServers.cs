using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using PestelLib.ClientConfig;
using PestelLib.Localization;
using UnityEngine;
using UnityDI;
using Debug = UnityEngine.Debug;
using PestelLib.UI;

public class PingServers : MonoBehaviour
{
    public static PhotonServerDescription PhotonServerDescription;
    public static int CurrentPing {
        get {
            return instance.GetServerResponseTime(PhotonServerDescription);
        }
    }
    static PingServers instance;

#pragma warning disable 649
    [Dependency] private Config _config;
    [Dependency] private Gui _gui;
    [Dependency] private LocalizationData _localization;
#pragma warning restore 649

    private readonly Dictionary<PhotonServerDescription, int> _pingTime = new Dictionary<PhotonServerDescription, int>();
    private volatile bool _finished = false;

    private GenericMessageBoxScreen _connectionProblemMessageBox;
    private const int TimeoutSeconds = 10;
    private float _runningTime;

    Thread _thread;

    void Start () {
        ContainerHolder.Container.BuildUp(this);
        PhotonServerDescription = _config.PhotonServers[0];
        TryPing();
        instance = this;
    }

    [ContextMenu("TryPing")]
    public void TryPing()
    {
        if (_thread != null) return;

        this.enabled = true;

        _runningTime = 0;
        _finished = false;
        _thread = new Thread(PingPhotonServers);
        _thread.Start();
    }

    void OnDestroy()
    {
        if (_thread != null)
        {
            _thread.Abort();
            _thread = null;
        }
    }

    void Update()
    {
        _runningTime += Time.deltaTime;

        if (_runningTime > TimeoutSeconds)
        {
            enabled = false;
            _thread = null;

            TryPing();
        }

        if (_finished)
        {
            enabled = false;
            _thread = null;
            PhotonServerDescription = GetFastestServer;
            Debug.Log(JsonConvert.SerializeObject(_pingTime));
        }
    }

    void PingPhotonServers()
    {
        var results = new Dictionary<PhotonServerDescription, int>();
        
        foreach (var photonServerDescription in _config.PhotonServers)
        {
            results[photonServerDescription] = TcpPingServer(photonServerDescription.Uri);
        }

        lock (_pingTime)
        {
            foreach (var result in results)
            {
                _pingTime[result.Key] = result.Value;
            }
        }

        _finished = true;
    }

    int TcpPingServer(string uri)
    {
        const int iterations = 3;
        const int timeout = 5000;

        var timeSum = 0;
        int currentIteration = 0;
        var startTime = DateTime.Now;

        while (currentIteration < iterations)
        {
            if ((DateTime.Now - startTime).TotalMilliseconds > timeout) return timeout;

            try
            {
                var adresses = Dns.GetHostAddresses(uri);
                if (adresses.Length == 0) return int.MaxValue;

                IPAddress ipAddress = adresses[0];
                IPEndPoint endPoint = new IPEndPoint(ipAddress, 80);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // Measure the Connect call only
                Socket sock;
                try
                {
                    sock = Connect(endPoint, AddressFamily.InterNetwork);
                }
#pragma warning disable 168
                catch (SocketException e)
#pragma warning restore 168
                {
                    //https://forum.unity.com/threads/ipv6-tcpclient-beginconnect-doesnt-work.412745/#post-2691696
                    //"My recommendation would be to try IPv4 first, then fall back to IPv6 if IPv4 is not available."
                    sock = Connect(endPoint, AddressFamily.InterNetworkV6);
                }

                stopwatch.Stop();

                int time = (int) stopwatch.Elapsed.TotalMilliseconds;
                timeSum += time;

                sock.Close();

                currentIteration++;
            }
            catch
            {
                // catch exception & retry if has network problems
            }

            Thread.Sleep(50);
        }
        return (int) timeSum/iterations;
    }
  
    private void ShowConnectionProblem()
    {
          _connectionProblemMessageBox = GenericMessageBoxScreen.Show(new GenericMessageBoxDef
        {
            Caption = _localization.Get("SystemConnectionProblemCaption"),
            Description = _localization.Get("SystemConnectionProblemDescription") + "\n Unable to ping photon servers",
            AutoHide = false,
            Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
            ButtonAAction = () =>
            {
                _gui.Close(_connectionProblemMessageBox.gameObject);
                TryPing();
            },
            ButtonALabel = _localization.Get("SystemConnectionProblemRetry")
        });
    }

    private static Socket Connect(IPEndPoint endPoint, AddressFamily addressFamily)
    {
        Socket sock;
        sock = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
        sock.Blocking = true;
        sock.Connect(endPoint);
        return sock;
    }

    public bool IsResultsReady
    {
        get { return _finished; }
    }

    public int GetServerResponseTime(PhotonServerDescription photonServerDescription)
    {
        lock (_pingTime)
        {
            if (_pingTime.ContainsKey(photonServerDescription))
            {
                return _pingTime[photonServerDescription];
            }
        }
        return 1000;
    }

    public PhotonServerDescription GetFastestServer
    {
        get
        {
            lock (_pingTime)
            {
                var fastestKey = _pingTime.Keys.First();
                var fastestValue = _pingTime[fastestKey];
                foreach (var i in _pingTime)
                {
                    if (i.Value < fastestValue)
                    {
                        fastestKey = i.Key;
                        fastestValue = i.Value;
                    }
                }
                return fastestKey;
            }
        }
    }
}
