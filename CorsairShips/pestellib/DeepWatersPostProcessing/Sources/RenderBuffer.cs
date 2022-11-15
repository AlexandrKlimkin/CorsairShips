using UnityEngine;

namespace PestelLib.DeepWatersPostProcessing
{
    public class RenderBuffer
    {

        public virtual bool RequireDepth { get; private set; }

        public virtual RenderTextureFormat Format { get; private set; }

        public virtual float Downsample { get; set; }

        public RenderTexture RenderTexture
        {
            get { return _RenderTexture; }
        }

        private RenderTexture _RenderTexture;

        public RenderBuffer()
        {
            RequireDepth = false;
            Format = RenderTextureFormat.Default;
            Downsample = 1;
        }

        public RenderBuffer(bool requireDepth, RenderTextureFormat format, float downsample)
        {
            RequireDepth = requireDepth;
            Format = format;
            Downsample = downsample;
        }

        public virtual bool RefreshRT(int width, int height)
        {
            if (_RenderTexture == null || _RenderTexture.width != (int) (width * Downsample) ||
                _RenderTexture.height != (int) (height * Downsample))
            {
                var oldTex = _RenderTexture;
                _RenderTexture = RenderTexture.GetTemporary((int) (width * Downsample), (int) (height * Downsample),
                    RequireDepth ? 16 : 0,
                    Format);
                _RenderTexture.filterMode = FilterMode.Bilinear;
                if (oldTex != null)
                    RenderTexture.ReleaseTemporary(oldTex);
                return true;
            }

            return false;
        }
    }
}