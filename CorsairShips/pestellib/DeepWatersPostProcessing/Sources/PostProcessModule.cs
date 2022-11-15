using UnityEngine;

namespace PestelLib.DeepWatersPostProcessing
{
    public abstract class PostProcessModule
    {

        public abstract string ShaderName { get; }

        protected Material RenderMaterial;

        private bool _Dirty = true;

        public void Initialize()
        {
            if (_Dirty || RenderMaterial == null)
            {
                _Dirty = false;
                RenderMaterial = new Material(Shader.Find(ShaderName));
                InitMaterial();
            }
        }

        protected virtual void InitMaterial()
        {

        }

        public void ProcessImage(RenderTexture destination, params RenderTexture[] source)
        {
            Initialize();
            OnProcessImage(destination, source);
        }

        protected abstract void OnProcessImage(RenderTexture destination, params RenderTexture[] source);
    }
}