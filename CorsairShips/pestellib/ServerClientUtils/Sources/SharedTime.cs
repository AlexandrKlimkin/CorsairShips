using System;
using PestelLib.ClientConfig;
using PestelLib.ServerShared;
using S;
using UnityDI;
using UnityEngine;
using PestelLib.Utils;

namespace PestelLib.ServerClientUtils
{
    public class SharedTime : MonoBehaviour, ITimeProvider
    {
#pragma warning disable 649
        [Dependency] private NetworkState _networkState;
        [Dependency] private RequestQueue _requestQueue;
        [Dependency] private Config _config;
#pragma warning restore 649

        public event Action OnTimeDesync = () => { };
        public event Action OnTimeSync = () => { };

        public bool Inited { get; private set; }
        public bool IsSynced { get { return !_isDesynced; } }
        public long TicksSinceLastSync { get { return timeSinceLastSync; } }

        private long lastSyncTimeStamp;
        private long timeSinceLastSync = 0;
        private long timeSinceLastSyncThreshold = 1800 * TimeSpan.TicksPerSecond; //Time must be synced every minute. If not - error will be shown
        private float syncTime = 60f;
        private float timeLastUpdate = 0f;

        private bool _isDesynced = true;
        private bool _isSuspended;
        private bool _syncInProgress;

        private void Start()
        {
            ContainerHolder.Container.BuildUp(this);
            enabled = false;
            if (_requestQueue != null)
            {
                _requestQueue.OnComplete += SyncTime;
                RequestSync();
            }
        }

        private void OnDestroy()
        {
            if (_requestQueue != null)
                _requestQueue.OnComplete -= SyncTime;
        }

        public virtual DateTime Now
        {
            get
            {
                if (_networkState != null && _networkState.OfflineMode)
                {
                    if (lastSyncTimeStamp != 0)
                    {
                        return new DateTime(lastSyncTimeStamp + timeSinceLastSync, DateTimeKind.Utc);
                    }
                    else
                    {
                        return DateTime.UtcNow;
                    }
                }

                if (_config == null || _config.UseLocalState) 
                {
                    return DateTime.UtcNow;
                }

                if (timeSinceLastSync > timeSinceLastSyncThreshold && !_isSuspended)
                {
                    if (!_isDesynced)
                    {
                        //Debug.LogWarning("There were no sync for " + timeSinceLastSync);
                         OnTimeDesync();
                        _isDesynced = true;
                    }
                    return LastSyncDateTime;
                }

                if (lastSyncTimeStamp == 0)
                {
                    return DateTime.UtcNow;
                }

                return new DateTime(lastSyncTimeStamp + timeSinceLastSync, DateTimeKind.Utc);
            }
        }

        public void SetSuspend(bool isSuspended)
        {
            _isSuspended = isSuspended;
        }

        private DateTime LastSyncDateTime
        {
            get
            {
                return new DateTime(lastSyncTimeStamp, DateTimeKind.Utc);
            }
        }
        
        private void SyncTime(Response response, DataCollection data)
        {
            lastSyncTimeStamp = response.Timestamp;
            //Debug.Log("Local timestamp = " + TimeUtils.ConvertToUnixTimestamp(DateTime.UtcNow) + " --- Server timestamp = " + lastSyncTimeStamp);

            timeSinceLastSync = 0;
            
            Invoke("RequestSync", syncTime);

            enabled = true;

            Inited = true;

            if (_isDesynced)
            {
                _isDesynced = false;
                OnTimeSync();
            }
        }

        private void Update()
        {
            float delta = Time.realtimeSinceStartup - timeLastUpdate;
            timeLastUpdate = Time.realtimeSinceStartup;
            
            timeSinceLastSync += (long)(delta * TimeSpan.TicksPerSecond);
        }

        private void RequestSync()
        {
            if (_isSuspended)
                return;

            if (!_syncInProgress)
            {
                _syncInProgress = true;

                _requestQueue.SendRequest("SyncTime", new Request
                {
                    SyncTime = new SyncTime()
                }, OnSyncComplete);
            }
        }

        private void OnSyncComplete(Response response, DataCollection dataCollection)
        {
            _syncInProgress = false;
        }

        private void DebugPrint()
        {
            Debug.Log("Time since last sync " + timeSinceLastSync);
            DateTime dateTimeLocal = DateTime.UtcNow;
            Debug.Log(string.Format("LocalTime: {0:D2}:{1:D2}:{2:D2}:", dateTimeLocal.Hour, dateTimeLocal.Minute, dateTimeLocal.Second));

            DateTime dateTimeServer = new DateTime(lastSyncTimeStamp + timeSinceLastSync, DateTimeKind.Utc);
            Debug.Log(string.Format("ServerTime: {0:D2}:{1:D2}:{2:D2}:", dateTimeServer.Hour, dateTimeServer.Minute, dateTimeServer.Second));
        }

        private void OnApplicationPause(bool pause) {
            if (!pause) {
                RequestSync();
            }
            else {
                OnTimeDesync();
                _isDesynced = true;
            }
        }
    }
}
