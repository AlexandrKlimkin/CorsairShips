using UnityDI;
using UnityEngine;

namespace PestelLib.Utils
{
    public class ParticlesCacheProxyAngleDependent : ParticlesCacheProxy
    {
        [Dependency] private CameraStack _cameraStack;

        [SerializeField] private AnimationCurve _angleToSize;

        private void Start()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        protected override void Update()
        {
            if (_cameraStack == null || _cameraStack.TopCamera == null)
            {
                return;
            }

            var dot = Mathf.Clamp01(Vector3.Dot(-transform.forward, _cameraStack.TopCamera.transform.forward));
            var evaluatedCoeff = _angleToSize.Evaluate(dot);

            var color = (Color)StartColor;
            color.a = evaluatedCoeff;
            
            _particlesCacheReference.Position = transform.position;
            _particlesCacheReference.Rotation = transform.eulerAngles;
            _particlesCacheReference.StartColor = color;
            _particlesCacheReference.StartSize = StartSize * evaluatedCoeff;
            _particlesCacheReference.ApplyStartSize = ApplyStartSize;
        }
    }
}