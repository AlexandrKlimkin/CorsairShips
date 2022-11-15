using UnityEngine;

namespace PestelLib.Utils
{
    [System.Serializable]
    public class ParticlesCacheReference
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public bool Killed;
        public bool ApplyRotation;
        public bool AutoRespawn;
        public Color32 StartColor;
        public float StartSize;
        public bool ApplyStartSize;
    }
}
