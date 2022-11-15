using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GlobalConflict.Example
{
    public class DragMe : MonoBehaviour
    {
        private Vector3 pos = Vector3.zero;
        private bool indrag;
        private RectTransform _rectTransform;

        // Use this for initialization
        void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            if (indrag)
            {
                transform.position += Input.mousePosition - pos;
                if (Input.GetMouseButtonUp(0))
                {
                    indrag = false;
                }
            }
            else
            {
                var mp = Input.mousePosition - _rectTransform.position;
                indrag = _rectTransform.rect.Contains(mp) && Input.GetMouseButtonDown(0);
            }

            pos = Input.mousePosition;
        }
    }
}