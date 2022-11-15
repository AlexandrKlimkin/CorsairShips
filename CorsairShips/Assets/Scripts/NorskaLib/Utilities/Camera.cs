using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NorskaLib.Utilities
{
    public struct CameraUtils
    {
        public static void Lerp(Camera from, Camera to, Camera camera, float t = 1)
        {
            camera.fieldOfView = Mathf.Lerp(from.fieldOfView, to.fieldOfView, t);
            camera.transform.position = Vector3.Lerp(from.transform.position, to.transform.position, t);
            camera.transform.rotation = Quaternion.Lerp(from.transform.rotation, to.transform.rotation, t);
        }
        public static void Lerp(CameraData from, CameraData to, Camera camera, float t = 1)
        {
            camera.fieldOfView = Mathf.Lerp(from.fieldOfView, to.fieldOfView, t);
            camera.transform.position = Vector3.Lerp(from.position, to.position, t);
            camera.transform.rotation = Quaternion.Lerp(from.rotation, to.rotation, t);
        }
        public static void Lerp(Camera from, CameraData to, Camera camera, float t = 1)
        {
            camera.fieldOfView = Mathf.Lerp(from.fieldOfView, to.fieldOfView, t);
            camera.transform.position = Vector3.Lerp(from.transform.position, to.position, t);
            camera.transform.rotation = Quaternion.Lerp(from.transform.rotation, to.rotation, t);
        }
        public static void Lerp(CameraData from, Camera to, Camera camera, float t = 1)
        {
            camera.fieldOfView = Mathf.Lerp(from.fieldOfView, to.fieldOfView, t);
            camera.transform.position = Vector3.Lerp(from.position, to.transform.position, t);
            camera.transform.rotation = Quaternion.Lerp(from.rotation, to.transform.rotation, t);
        }
    }

    public struct CameraData
    {
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly float fieldOfView;

        public CameraData(Vector3 position, Quaternion rotation, float fieldOfView) : this()
        {
            this.position = position;
            this.rotation = rotation;
            this.fieldOfView = fieldOfView;
        }

        public CameraData(Camera camera)
        {
            this.position = camera.transform.position;
            this.rotation = camera.transform.rotation;
            this.fieldOfView = camera.fieldOfView;
        }
    }
}
