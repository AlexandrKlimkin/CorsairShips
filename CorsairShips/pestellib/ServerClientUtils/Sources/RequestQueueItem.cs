using System;
using PestelLib.ServerShared;
using S;
using UnityEngine;

namespace PestelLib.ServerClientUtils
{
    public class RequestQueueItem
    {
        public string Method;
        public byte[] PostData;
        public BestHTTP.HTTPRequest Loader;
        public float LoadingBeginTimestamp;
        public float NextAttemptTimestamp;
        public Action<Response, DataCollection> OnComplete = (t, d) => { };
        public int FailureCount = 0;
        public DateTime? SendTimstamp;

        public Request Header;
        public byte[] Data;
        public byte[] State;

        public bool Finished;
    }
}
