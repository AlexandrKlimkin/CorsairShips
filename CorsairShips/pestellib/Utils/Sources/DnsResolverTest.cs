using System;
using System.Net;
using UnityEngine;
using System.Threading;

namespace PestelLib.Utils
{
    public class DnsResolverTest : MonoBehaviour
    {
        bool _threadRunning;
        Thread _thread;

        private string _uri;
        private Action<bool> _onResult;
        private Exception _exception;

        public static void TestHost(string hostName, Action<bool> onResult)
        {
            var go = new GameObject("DnsResolverTest");
            var instance = go.AddComponent<DnsResolverTest>();
            instance._uri = hostName;
            instance._onResult = onResult;
            DontDestroyOnLoad(go);
        }

        void Start()
        {
            _thread = new Thread(ThreadedWork);
            _thread.Start();
        }

        void ThreadedWork()
        {
            _threadRunning = true;

            try
            {
                var host = Dns.GetHostEntry(_uri);
                _exception = null;
            }
            catch (Exception e)
            {
                _exception = e;
            }

            _threadRunning = false;
        }

        void Update()
        {
            if (!_threadRunning)
            {
                if (_exception == null)
                {
                    _onResult(true);
                }
                else
                {
                    Debug.LogError(_exception);
                    _onResult(false);
                }
                Destroy(gameObject);
            }
        }

        void OnDisable()
        {
            // If the thread is still running, we should shut it down,
            // otherwise it can prevent the game from exiting correctly.
            if (_threadRunning)
            {
                // This forces the while loop in the ThreadedWork function to abort.
                _threadRunning = false;

                // This waits until the thread exits,
                // ensuring any cleanup we do after this is safe. 
                _thread.Join();
            }

            // Thread is guaranteed no longer running. Do other cleanup tasks.
        }
    }
}