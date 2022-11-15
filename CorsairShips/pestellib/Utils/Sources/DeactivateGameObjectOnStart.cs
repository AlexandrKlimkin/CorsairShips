using UnityEngine;

namespace PestelLib.Utils
{
    public class DeactivateGameObjectOnStart : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(false);
        }
    }
}