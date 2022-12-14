using System.Collections;
using UnityEngine;

public static class CoroutineExtensions {
    public static void StartCoroutineSafe(this MonoBehaviour monoBehaviour, IEnumerator enumerator, ref Coroutine coroutine) {
        if (!monoBehaviour.gameObject.activeInHierarchy) {
            Debug.LogWarning("can't start coroutine, gameObject is not active");
            return;
        }
        if (coroutine != null)
            monoBehaviour.StopCoroutine(coroutine);
        coroutine = monoBehaviour.StartCoroutine(enumerator);
    }
    
    public static void StopCoroutineSafe(this MonoBehaviour monoBehaviour, ref Coroutine coroutine) {
        if (coroutine != null)
            monoBehaviour.StopCoroutine(coroutine);
        coroutine = null;
    }
}