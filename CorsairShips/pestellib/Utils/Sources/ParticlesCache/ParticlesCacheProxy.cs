using UnityDI;
using UnityEngine;
using UnityEngine.Serialization;

namespace PestelLib.Utils
{
    public class ParticlesCacheProxy : MonoBehaviour
    {
        [SerializeField] private bool AutoRespawn;
        [SerializeField] private ParticlesCache _particlesCache;
        [FormerlySerializedAs("ParticleSystemName")]
        [SerializeField] private string _particleSystemName;
        [SerializeField] private bool _applyRotation;

        public Color32 StartColor = Color.white;
        public float StartSize = 1f;
        public bool ApplyStartSize = true;

        protected ParticlesCacheReference _particlesCacheReference;

        void Awake()
        {
            if (_particlesCache == null)
            {
                _particlesCache = ContainerHolder.Container.Resolve<ParticlesCache>();
            }
        }

        void OnEnable()
        {
            _particlesCacheReference = _particlesCache.AddParticle(_particleSystemName, transform.position, transform.eulerAngles, _applyRotation, StartColor);
            _particlesCacheReference.AutoRespawn = AutoRespawn;
        }

        void OnDisable()
        {
            if (_particlesCacheReference != null)
            {
                _particlesCacheReference.Killed = true;
                _particlesCacheReference = null;
            }
        }

        protected virtual void Update()
        {
            _particlesCacheReference.Position = transform.position;
            _particlesCacheReference.Rotation = transform.eulerAngles;
            _particlesCacheReference.StartColor = StartColor;
            _particlesCacheReference.StartSize = StartSize;
            _particlesCacheReference.ApplyStartSize = ApplyStartSize;
        }
    }
}
