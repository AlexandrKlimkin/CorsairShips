using System;
using System.Collections;
using System.Threading;
using PestelLib.ClientConfig;
using PestelLib.NetworkUtils.Sources.InternetReachability;
using UnityEngine;
using UnityDI;

namespace PestelLib.NetworkUtils
{
    public class InternetReachabilityBackendTcpPing : MonoBehaviour, IInternetReachability
    {
        static readonly Thread mainThread = Thread.CurrentThread;

        [Dependency] private Config _config;

        private bool _internetState = false;
        public float CheckPeriod = 10f;
        public float Timeout = 3f;

        private ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private string _pingUri;

        private IEnumerator Start()
        {
            Uri myUri = new Uri(_config.DynamicURL);
            _pingUri = "https://" + myUri.Host;

            while (true)
            {
                var requestStartTimestamp = Time.realtimeSinceStartup;
                var www = new WWW(_pingUri);
                while (!www.isDone 
                       && string.IsNullOrEmpty(www.error) 
                       && (Time.realtimeSinceStartup - requestStartTimestamp) < Timeout)
                {
                    yield return null;
                }

                _internetState = string.IsNullOrEmpty(www.error);
                yield return new WaitForSecondsRealtime(CheckPeriod);
            }
        }

        public bool HasInternet
        {
            get { return _internetState; }
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

        private void Update()
        {
            if (HasInternet)
                _manualResetEvent.Set();
            else
                _manualResetEvent.Reset();
        }
    }
}