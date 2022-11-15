using UnityEngine;

namespace UnityStandardAssets.CrossPlatformInput
{
    public class ButtonHandler : MonoBehaviour
    {
        [SerializeField] private KeyCode StandaloneButton = KeyCode.LeftControl;

        public string Name;

        public void SetDownState()
        {
            CrossPlatformInputManager.SetButtonDown(Name);
        }


        public void SetUpState()
        {
            CrossPlatformInputManager.SetButtonUp(Name);
        }


        public void SetAxisPositiveState()
        {
            CrossPlatformInputManager.SetAxisPositive(Name);
        }


        public void SetAxisNeutralState()
        {
            CrossPlatformInputManager.SetAxisZero(Name);
        }


        public void SetAxisNegativeState()
        {
            CrossPlatformInputManager.SetAxisNegative(Name);
        }

        public void Update()
        {
            if (Input.GetKeyDown(StandaloneButton))
            {
                SetDownState();
            }

            if (Input.GetKeyUp(StandaloneButton))
            {
                SetUpState();
            }
        }
    }
}
