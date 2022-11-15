using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NorskaLib.Utilities
{
    /// <summary>
    /// Contains device-independent pointer checks.
    /// </summary>
    public struct InputUtils
    {
        public static bool PointerIsDown
        {
            get
            {
                switch (SystemInfo.deviceType)
                {
                    default:
                    case DeviceType.Console:
                    case DeviceType.Unknown:
                        return false;

                    case DeviceType.Handheld:
                        if (UnityEngine.Input.touchCount > 0)
                            switch (UnityEngine.Input.GetTouch(0).phase)
                            {
                                case TouchPhase.Began:
                                case TouchPhase.Moved:
                                case TouchPhase.Stationary:
                                    return true;
                                case TouchPhase.Ended:
                                case TouchPhase.Canceled:
                                default:
                                    return false;

                            }
                        else
                            return false;

                    case DeviceType.Desktop:
                        return UnityEngine.Input.GetMouseButtonDown(0)
                            || UnityEngine.Input.GetMouseButton(0);
                }
            }
        }

        public static Vector2 PointerPosition
        {
            get
            {
                switch (SystemInfo.deviceType)
                {
                    default:
                    case DeviceType.Console:
                    case DeviceType.Unknown:
                        return Vector2.zero;

                    case DeviceType.Handheld:
                        if (UnityEngine.Input.touchCount > 0)
                            return UnityEngine.Input.GetTouch(0).position;
                        else
                            return Vector2.zero;


                    case DeviceType.Desktop:
                        return (Vector2)UnityEngine.Input.mousePosition;
                }
            }
        }
    }
}
