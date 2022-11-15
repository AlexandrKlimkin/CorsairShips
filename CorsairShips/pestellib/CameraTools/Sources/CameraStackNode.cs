using UnityDI;
using UnityEngine;

public class CameraStackNode : InitableMonoBehaviour
{
    [Dependency] private CameraStack _stack = null;

    public bool _activateOnStart = false;

    override protected void SafeStart()
    {
        if (_activateOnStart)
        {
            _stack.Push(GetComponent<Camera>());
        }
        else
        {
            GetComponent<AudioListener>().enabled = false;
            GetComponent<Camera>().enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (_stack != null)
        {
            _stack.RemoveCamera(GetComponent<Camera>());
        }
    }
}