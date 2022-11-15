using System.Collections;
using UnityEngine;

public class CardTimer : MonoBehaviour
{
	public float _Timer;

    IEnumerator Start()
    {
        for (var i = 1; i < transform.childCount; i++) //0 element is prefab, so start from 1
        {
            transform.GetChild(i).gameObject.SetActive(true);
            yield return new WaitForSeconds(_Timer);
        }
    }
}