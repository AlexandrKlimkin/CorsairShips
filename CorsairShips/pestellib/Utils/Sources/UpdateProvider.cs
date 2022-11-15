using System;
using ServerShared;
using UnityEngine;

namespace PestelLib.Utils
{
    public class UpdateProvider : MonoBehaviour, IUpdateProvider
    {
        public event Action OnUpdate = () => { };

        private void Update()
        {
            OnUpdate();
        }
    }
}
