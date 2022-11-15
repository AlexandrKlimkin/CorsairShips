using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NorskaLib.Extensions
{
    public static class ComponentExtensions
    {
        public static void Mimic<T>(this T @this, T other) where T : Component
        {
            if (@this is Behaviour thisB)
            {
                var otherB = other as Behaviour;
                thisB.enabled = otherB.enabled;
            }

            switch (@this)
            {
                case RectTransform thisRT:
                    var otherRT = other as RectTransform;

                    thisRT.pivot                    = otherRT.pivot;
                    thisRT.anchorMin                = otherRT.anchorMin;
                    thisRT.anchorMax                = otherRT.anchorMax;
                    thisRT.sizeDelta                = otherRT.sizeDelta;
                    thisRT.anchoredPosition         = otherRT.anchoredPosition;

                    break;

                case ContentSizeFitter thisCSF:
                    var otherCSF = other as ContentSizeFitter;

                    thisCSF.horizontalFit            = otherCSF.horizontalFit;
                    thisCSF.verticalFit              = otherCSF.verticalFit;

                    break;

                case HorizontalOrVerticalLayoutGroup thisLG:
                    var otherLG = other as HorizontalOrVerticalLayoutGroup;

                    thisLG.childAlignment           = otherLG.childAlignment;
                    thisLG.padding                  = otherLG.padding;
                    thisLG.spacing                  = otherLG.spacing;
                    thisLG.reverseArrangement       = otherLG.reverseArrangement;

                    thisLG.childControlHeight       = otherLG.childControlHeight;
                    thisLG.childControlWidth        = otherLG.childControlWidth;
                    thisLG.childForceExpandHeight   = otherLG.childForceExpandHeight;
                    thisLG.childForceExpandWidth    = otherLG.childForceExpandWidth;
                    thisLG.childScaleHeight         = otherLG.childScaleHeight;
                    thisLG.childScaleWidth          = otherLG.childScaleWidth;

                    break;

                default:
                    throw new System.NotImplementedException();
                    //break;
            }
        }
    }
}