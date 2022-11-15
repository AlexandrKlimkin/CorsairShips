using System;
using System.Collections;
using UnityEngine;

//http://answers.unity3d.com/questions/301868/yield-waitforseconds-outside-of-timescale.html

namespace PestelLib.Utils
{
    public static class CoroutineUtil
    {
        public static IEnumerator WaitForRealSeconds(float time)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < start + time)
            {
                yield return null;
            }
        }

        public static void WaitForEndFrame(this MonoBehaviour gameObject, Action callBack)
        {
            gameObject.StartCoroutine(WaitForEndFrameCoroutine(callBack));
        }

        private static IEnumerator WaitForEndFrameCoroutine(Action callBack)
        {
            yield return new WaitForEndOfFrame();

            if (callBack != null)
            {
                callBack();
            }
        }

        public static void DelayMethod(this MonoBehaviour gameObject, float delay, Action callBack)
        {
            gameObject.StartCoroutine(WaitForDelay(delay, callBack));
        }

        private static IEnumerator WaitForDelay(float delay, Action callBack)
        {
            yield return new WaitForSeconds(delay);

            if (callBack != null)
            {
                callBack();
            }
        }
    }
}
