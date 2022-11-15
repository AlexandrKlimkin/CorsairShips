using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.UI
{
    public class CanvasBuilder
    {
        public static Canvas CreateCanvas(CanvasSetup setup)
        {
            var go = new GameObject("PestelLib.Canvas");

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = setup.RenderMode;
            canvas.pixelPerfect = setup.PixelPerfect;
            canvas.sortingOrder = setup.SortOrder;
            canvas.targetDisplay = setup.TargetDisplay;
            canvas.additionalShaderChannels = setup.AdditionalCanvasShaderChannels;

            var canvasScaler = go.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = setup.ScaleMode;
            canvasScaler.referenceResolution = setup.ReferenceResolution;
            canvasScaler.screenMatchMode = setup.ScreenMatchMode;
            canvasScaler.matchWidthOrHeight = setup.MatchWidthOrHeight;
            canvasScaler.referencePixelsPerUnit = setup.ReferencePixelsPerUnit;

            var graphicRaycaster = go.AddComponent<GraphicRaycaster>();
            graphicRaycaster.ignoreReversedGraphics = setup.IgnoreReversedGraphics;
            graphicRaycaster.blockingObjects = setup.BlockingObjects;

            return canvas;
        }
    }
}