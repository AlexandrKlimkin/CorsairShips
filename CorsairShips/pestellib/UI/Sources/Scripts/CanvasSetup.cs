using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.UI
{
    [System.Serializable]
    public class CanvasSetup
    {
        [Header("Canvas")] public RenderMode RenderMode = RenderMode.ScreenSpaceOverlay;
        public bool PixelPerfect = false;
        public int SortOrder = 100;
        public int TargetDisplay = 0;
        public AdditionalCanvasShaderChannels AdditionalCanvasShaderChannels = AdditionalCanvasShaderChannels.None;

        [Header("Canvas Scaler")] public CanvasScaler.ScaleMode ScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        public Vector2 ReferenceResolution = new Vector2(1920, 1080);
        public CanvasScaler.ScreenMatchMode ScreenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        public float MatchWidthOrHeight = 1.0f;
        public float ReferencePixelsPerUnit = 100f;

        [Header("Graphic Raycaster")] public bool IgnoreReversedGraphics = true;
        public GraphicRaycaster.BlockingObjects BlockingObjects = GraphicRaycaster.BlockingObjects.None;
    }
}