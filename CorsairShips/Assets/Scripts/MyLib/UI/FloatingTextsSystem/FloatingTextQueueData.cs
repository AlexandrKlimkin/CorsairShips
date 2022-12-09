using System;
using UnityEngine;

namespace UI.FloatingTexts {
    [Serializable]
    public struct FloatingTextQueueData {
        public QueueType QueueType;
        [Range(0.05f, 10f)]
        public float Delay;
    }

    public enum QueueType {
        None,
        Notifications,
    }
}