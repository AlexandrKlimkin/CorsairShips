using System;
using UnityEngine;

namespace PestelLib.DeepWatersPostProcessing
{
    [Serializable]
    public class SelectionModule : PostProcessModule
    {

        public override string ShaderName
        {
            get { return "Hidden/PostProcess/Selection"; }
        }

        public Color FillColor;
        public Texture FillPattern;
        [Range(0, 5)] public float FillPatternSpeed = 2;
        [Range(0, 5)] public float OutlineHardness = 3;

        private int _OutlineHardnessField;
        private int _FillColorField;
        private int _FillPatternField;
        private int _FillPatternSpeedField;


        protected override void InitMaterial()
        {
            _OutlineHardnessField = Shader.PropertyToID("_OutlineHardness");
            _FillColorField = Shader.PropertyToID("_FillColor");
            _FillPatternField = Shader.PropertyToID("_FillPattern");
            _FillPatternSpeedField = Shader.PropertyToID("_FillPatternSpeed");
        }

        protected override void OnProcessImage(RenderTexture destination, RenderTexture[] source)
        {
            RenderMaterial.SetFloat(_OutlineHardnessField, OutlineHardness);
            RenderMaterial.SetFloat(_FillPatternSpeedField, FillPatternSpeed);
            RenderMaterial.SetColor(_FillColorField, FillColor);
            RenderMaterial.SetTexture(_FillPatternField, FillPattern);

            var mainBuffer = source[0];
            Graphics.Blit(mainBuffer, destination, RenderMaterial);
        }
    }
}