using System;
using System.IO;
using UnityDI;
using UnityEngine;

namespace PestelLib.DeepWatersPostProcessing
{
    [DisallowMultipleComponent, ExecuteInEditMode, ImageEffectAllowedInSceneView]
    [AddComponentMenu("Rendering/Post Process Controller", -1)]
    [RequireComponent(typeof(Camera))]
    public class PostProcessController : MonoBehaviour
    {

        [Dependency] protected IQualitySettingsManager _qualityManager;

        public ColorCorrectionModule ColorCorrection = new ColorCorrectionModule();

        public float Aberration
        {
            set { ColorCorrection.Aberration = value; }
            get { return ColorCorrection.Aberration; }
        }

        private void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
    #endif
            if (_qualityManager == null)
            {
                Debug.Log("Please register IQualitySettingsManager in ContainerHolder.Container");
                return;
            }

            _qualityManager.OnChangeQuality += QualityManagerOnOnChangeQuality;
            QualityManagerOnOnChangeQuality(_qualityManager.CurrentQuality);
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
    #endif
            if (_qualityManager != null)
            {
                _qualityManager.OnChangeQuality -= QualityManagerOnOnChangeQuality;
            }
        }

        private void QualityManagerOnOnChangeQuality(int i)
        {
            this.enabled = i > 0;
        }

#if UNITY_EDITOR
        private bool _PendingSnapshot;
#endif

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
#if UNITY_EDITOR
                _PendingSnapshot = true;
#endif
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (ModulesReady())
            {
                //Blur.ProcessImage(_BlurBuffer.RenderTexture, source);
                //Bloom.ProcessImage(_BloomBuffer.RenderTexture, source, _BlurBuffer.RenderTexture);
                //ColorCorrection.ProcessImage(destination, _BloomBuffer.RenderTexture);

                ColorCorrection.ProcessImage(destination, source);

#if UNITY_EDITOR
                if (_PendingSnapshot) {

                    var cam = this.GetComponent<Camera>();
                    var buffer = new RenderBuffer(false, RenderTextureFormat.ARGB32, 1);
                    buffer.RefreshRT(source.width, source.height);
                    ColorCorrection.ProcessImage(buffer.RenderTexture, source);
                    Debug.Log(buffer.RenderTexture);
                    SaveSnapshot(buffer.RenderTexture);
                    _PendingSnapshot = false;
                }
#endif
            }
        }
#if UNITY_EDITOR
        private void SaveSnapshot(RenderTexture rt) {
            var oldRT = RenderTexture.active;

            Debug.Log(Application.dataPath);
            var path =
 Application.dataPath.Replace("/Assets", "/Snapshots") + "/" + DateTime.Now.ToLongTimeString().Replace(' ', '_').Replace(':', '_') + ".png";
            Debug.Log(path);
            var tex = new Texture2D(rt.width, rt.height);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            FileInfo file = new FileInfo(path);
            file.Directory.Create(); // If the directory already exists, this method does nothing.
            File.WriteAllBytes(file.FullName, tex.EncodeToPNG());
            RenderTexture.active = oldRT;
        }
#endif

        private bool ModulesReady()
        {
            return
                ColorCorrection != null;
        }
    }
}