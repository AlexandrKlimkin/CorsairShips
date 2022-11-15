using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraStack : MonoBehaviour
{
    public Action<Camera, bool> OnCameraChanged = (c, b) => { };

    [SerializeField] private List<Camera> Cameras = new List<Camera>();

    public void Push(Camera cam)
    {
        if (cam == null)
        {
            Debug.LogError("Can't push null to camera stack");
            return;
        }

        if (Cameras.Contains(cam))
        {
            Cameras.Remove(cam);
        }

        SetActiveCameraState(false);
        Cameras.Add(cam);
        SetActiveCameraState(true);
    }

    private void Pop()
    {
        if (Cameras.Count < 2)
        {
            return;
        }

        SetActiveCameraState(false);
        Cameras.RemoveAt(Cameras.Count - 1);
        SetActiveCameraState(true);
    }

    public void RemoveCamera(Camera cam)
    {
        var index = Cameras.IndexOf(cam);
        if (index == Cameras.Count - 1)
        {
            Pop();
        }
        else if (index != -1)
        {
            Cameras.RemoveAt(index);
        }
    }

    public Camera TopCamera
    {
        get { return Cameras.Count > 0 ? Cameras[Cameras.Count - 1] : null; }
    }

    private void SetActiveCameraState(bool state)
    {
        if (TopCamera != null)
        {
            OnCameraChanged(TopCamera, state);
            TopCamera.enabled = state;

            var listener = TopCamera.GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = state;
            }
        }
    }
}
