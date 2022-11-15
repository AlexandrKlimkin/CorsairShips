using System;
using UnityEngine;

namespace PestelLib.DeepWatersPostProcessing
{
    [Serializable]
    public class BloomModule : PostProcessModule
    {

        public override string ShaderName
        {
            get { return "Hidden/PostProcess/Bloom"; }
        }

        [Range(0, 1)] public float Intensity;
        [Range(0, 1)] public float Threshold;

        private int _BlurTexField;
        private int _IntensityField;
        private int _ThresholdField;

        protected override void InitMaterial()
        {
            _BlurTexField = Shader.PropertyToID("_BlurTex");
            _IntensityField = Shader.PropertyToID("_Intensity");
            _ThresholdField = Shader.PropertyToID("_Threshold");
        }

        protected override void OnProcessImage(RenderTexture destination, RenderTexture[] source)
        {
            var mainBuffer = source[0];
            var blurBuffer = source[1];
            RenderMaterial.SetTexture(_BlurTexField, blurBuffer);
            RenderMaterial.SetFloat(_IntensityField, Intensity);
            RenderMaterial.SetFloat(_ThresholdField, Threshold);
            Graphics.Blit(mainBuffer, destination, RenderMaterial);
        }
    }
}