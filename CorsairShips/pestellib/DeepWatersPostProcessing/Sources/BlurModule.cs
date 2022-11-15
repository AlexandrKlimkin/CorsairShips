using System;
using System.Collections.Generic;
using UnityEngine;

namespace PestelLib.DeepWatersPostProcessing
{
    [Serializable]
    public class BlurModule : PostProcessModule
    {

        public override string ShaderName
        {
            get { return "Hidden/PostProcess/GaussianBlur"; }
        }


        public bool GaussianBlur;
        [Range(0, 1)] public float Intensity;
        public float MaxDownsample = 0.15f;

        private const int MaxRadius = 10;

        private Dictionary<int, BlurKernel> _KernelCache = new Dictionary<int, BlurKernel>();

        protected override void OnProcessImage(RenderTexture destination, RenderTexture[] source)
        {
            var mainBuffer = source[0];
            if (GaussianBlur)
            {
                var radius = (int) (MaxRadius * Intensity);
                if (radius == 0)
                {
                    Graphics.Blit(mainBuffer, destination);
                    return;
                }

                var downsample = MaxDownsample + (1 - MaxDownsample) * (1 - Intensity) * (1 - Intensity);
                var rt1 = RenderTexture.GetTemporary((int) (mainBuffer.width * downsample),
                    (int) (mainBuffer.height * downsample), 0, RenderTextureFormat.ARGB32);
                var rt2 = RenderTexture.GetTemporary((int) (mainBuffer.width * downsample),
                    (int) (mainBuffer.height * downsample), 0, RenderTextureFormat.ARGB32);
                rt1.name = "BlurRT1";
                rt2.name = "BlurRT2";
                var kernel = GetKernel(radius);
                RenderMaterial.SetInt("_Iterations", kernel.Iterations);
                RenderMaterial.SetFloatArray("_Weight", kernel.Weights);
                RenderMaterial.SetFloatArray("_Offset", kernel.Offsets);
                Graphics.Blit(mainBuffer, rt1);
                Graphics.Blit(rt1, rt2, RenderMaterial, 0);
                Graphics.Blit(rt2, rt1, RenderMaterial, 1);
                Graphics.Blit(rt1, destination);
                RenderTexture.ReleaseTemporary(rt1);
                RenderTexture.ReleaseTemporary(rt2);
            }
            else
            {
                Graphics.Blit(mainBuffer, destination);
            }
        }



        private BlurKernel GetKernel(int radius)
        {
            if (!_KernelCache.ContainsKey(radius))
            {
                _KernelCache.Add(radius, GenerateKernel(radius));
            }

            return _KernelCache[radius];
        }

        // TODO: Move BlurKernel generation to a BlurUtil class
        public static BlurKernel GenerateKernel(int radius)
        {
            var result = new float[radius + 1];
            var sum = 0.0f;
            for (int i = 0; i < radius + 1; i++)
            {
                result[i] = CalculateKernelElement(i, 0, radius);
                for (int j = 1; j < radius + 1; j++)
                {
                    result[i] += CalculateKernelElement(i, j, radius);
                }

                sum += result[i] * ((i == 0) ? 1 : 2);
            }

            for (int i = 0; i < radius + 1; i++)
            {
                result[i] = result[i] / sum;
            }

            return OptimizeKernel(result, radius);
        }

        private static float CalculateKernelElement(int x, int y, int radius)
        {
            float sigma = (float) radius / 3;
            return 1 / (2 * Mathf.PI * sigma * sigma) * Mathf.Exp(-(x * x + y * y) / (2 * sigma * sigma));
        }

        private static BlurKernel OptimizeKernel(float[] kernel, int radius)
        {
            var iterations = Mathf.CeilToInt((float) (radius) / 2) + 1;
            var weights = new float[radius + 1];
            var offsets = new float[radius + 1];
            var index = 1;
            weights[0] = kernel[0];
            offsets[0] = 0;
            if (radius % 2 != 0)
            {
                weights[1] = kernel[1];
                offsets[1] = 1;
                index = 2;
            }

            if (radius > 1)
            {
                var j = index;
                for (int i = index; i < iterations; i++)
                {
                    weights[i] = kernel[j] + kernel[j + 1];
                    offsets[i] = (j * kernel[j] + (j + 1) * kernel[j + 1]) / weights[i];
                    j += 2;
                }
            }

            // Debug: info
            //Debug.Log(radius);
            //Debug.Log(iterations);
            //Debug.Log(String.Join(" ; ", kernel.Select(_ => _.ToString()).ToArray()));
            //Debug.Log(String.Join(" ; ", weights.Select(_ => _.ToString()).ToArray()));
            //Debug.Log(String.Join(" ; ", offsets.Select(_ => _.ToString()).ToArray()));

            return new BlurKernel() {Iterations = iterations, Weights = weights, Offsets = offsets};
        }

        public class BlurKernel
        {
            public int Iterations;
            public float[] Weights;
            public float[] Offsets;
        }
    }
}