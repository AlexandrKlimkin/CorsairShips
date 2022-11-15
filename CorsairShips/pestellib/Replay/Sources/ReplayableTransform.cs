using UnityEngine;

namespace PestelLib.Replay
{
    public struct ReplayableTransform
    {
        public int FrameNumber;
        public float Time;
        public Vector3 Position;
        public Quaternion Rotation;
    }
}
