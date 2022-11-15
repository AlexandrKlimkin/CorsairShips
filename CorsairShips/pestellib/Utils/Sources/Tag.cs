using UnityEngine;

namespace PestelLib.Utils
{
    public class Tag : MonoBehaviour
    {
        public string[] Tags = new string[0];

        public void Awake()
        {
            TagRegistry.AddObject(this);
        }

        public void OnDestroy()
        {
            TagRegistry.RemoveObject(this);
        }
    }
}