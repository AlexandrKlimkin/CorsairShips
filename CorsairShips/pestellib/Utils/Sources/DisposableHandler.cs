using System;
using UnityEngine;

namespace PestelLib.Utils
{
    public class DisposableHandler : MonoBehaviour
    {
        public IDisposable Content;
        
        void OnDestroy()
        {
            if (Content != null)
            {
                Content.Dispose();
            }
        }
    }
}