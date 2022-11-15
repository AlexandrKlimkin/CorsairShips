using System;
using UnityEngine;

namespace PestelLib.DeepWatersPostProcessing
{
    [Serializable]
    public class ColorCorrectionModule : PostProcessModule
    {

        public override string ShaderName
        {
            get { return "Hidden/PostProcess/ColorCorrection"; }
        }

        public Texture2D LUT;
        [Range(0, 1)] public float Aberration;
        [Range(0, 1)] public float Desaturation;

        //[Range(0, 1)]
        //public float VignetteAmmount;
        //[Range(0, 50)]
        //public float VignettePower;

        private float _LastAberration;

        private int _LUTField;
        private int _LUTParamsField;
        private int _AberrationField;
        private int _DesaturationField;

        //private int _VignetteAmmountField;
        //private int _VignettePowerField;

        protected override void InitMaterial()
        {
            _LUTField = Shader.PropertyToID("_LUT");
            _LUTParamsField = Shader.PropertyToID("_LUTParams");
            _AberrationField = Shader.PropertyToID("_Aberration");
            _DesaturationField = Shader.PropertyToID("_Desaturation");
            //_VignetteAmmountField = Shader.PropertyToID("_VignetteAmmount");
            //_VignettePowerField = Shader.PropertyToID("_VignettePower");
        }

        protected override void OnProcessImage(RenderTexture destination, RenderTexture[] source)
        {
            var mainBuffer = source[0];
            RenderMaterial.SetTexture(_LUTField, LUT);
            RenderMaterial.SetVector(_LUTParamsField, new Vector3(1f / LUT.width, 1f / LUT.height, LUT.height - 1f));
            RenderMaterial.SetFloat(_AberrationField, Aberration);
            RenderMaterial.SetFloat(_DesaturationField, Desaturation);

            if (_LastAberration == 0 && Aberration > 0)
            {
                RenderMaterial.EnableKeyword("ABERRATION_ON");
            }

            if (_LastAberration > 0 && Mathf.Approximately(Aberration, 0))
            {
                RenderMaterial.DisableKeyword("ABERRATION_ON");
            }

            _LastAberration = Aberration;

            //RenderMaterial.SetFloat(_VignetteAmmountField, VignetteAmmount);
            //RenderMaterial.SetFloat(_VignettePowerField, VignettePower);
            Graphics.Blit(mainBuffer, destination, RenderMaterial);
        }
    }
}