using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.UI
{
    public class PopUpBackgroundBuilder
    {
        public static CanvasGroupFader MakePopUpBackground(PopUpBackgroundSetup setup)
        {
            var go = new GameObject("PestelLib.PopUpBackground");

            var image = go.AddComponent<Image>();
            image.color = setup.Color;

            var canvasGroup = go.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.ignoreParentGroups = false;

            var canvasGroupFader = go.AddComponent<CanvasGroupFader>();
            canvasGroupFader._canvasGroup = canvasGroup;

            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;

            return canvasGroupFader;
        }
    }
}