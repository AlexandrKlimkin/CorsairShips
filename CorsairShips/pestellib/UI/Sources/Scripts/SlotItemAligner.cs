using UnityEngine;

namespace PestelLib.UI
{
    public class SlotItemAligner : MonoBehaviour
    {
        [SerializeField] private Transform[] _slots;

        [SerializeField] private bool _setSize = false;

        public Transform[] Slots
        {
            get { return _slots; }
        }
        
        private Transform[] _overridenSlots;

        public void OverrideSlots(Transform[] slots)
        {
            _overridenSlots = slots;
            Align();
        }

        public void Update()
        {
            Align();
        }

        public void LateUpdate()
        {
            Align();
        }

        private void Align()
        {
            var slots = (_overridenSlots != null && _overridenSlots.Length > 0) ? _overridenSlots : _slots;
            for (int i = 0; i < transform.childCount && i < slots.Length; i++)
            {
                var child = (RectTransform) transform.GetChild(i);
                var slot = (RectTransform)slots[i];
                child.position = slot.position;
                child.rotation = slot.rotation;
                if (_setSize)
                {
                    child.sizeDelta = slot.sizeDelta;
                }
            }
        }
    }
}
