using System;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace PestelLib.QualitySwitcher
{
    public class QualitySettingsManager : MonoBehaviour, IQualitySettingsManager
    {
        private const string SavedQualityLevelKey = "SavedQualityLevel";
        private const string IsQualityOverridedKey = "IsQualityOverrided";

        public event Action<int> OnChangeQuality = level => { };

        protected void Start()
        {
            if (!IsQualityOverrided)
            {
                //Detect default quality settings only once
                SetQualityByUserChoice(CurrentQuality);
            }
            UpdateQuality();
        }

        private void UpdateQuality()
        {
            try
            {
                Shader.globalMaximumLOD = CurrentQuality > 0 ? 1000 : 400;
                //Debug.Log("Shader LOD: " + Shader.globalMaximumLOD);
                OnChangeQuality(CurrentQuality);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n " + e.StackTrace);
            }
        }

        public int CurrentQuality
        {
            get
            {
                if (IsQualityOverrided)
                {
                    //Debug.Log("Quality overrided to " + SavedQualityLevel);
                    return SavedQualityLevel;
                }
                else
                {
                    //if (IsLowEndDevice)
                    //    Debug.Log("LowEndDevice");
                    //else if (IsMediumDevice)
                    //    Debug.Log("MediumDevice");
                    //else
                    //    Debug.Log("HighEndDevice");

                    if (Application.isEditor)
                    {
                        return QualitySettings.GetQualityLevel();
                    }

#if UNITY_ANDROID && !UNITY_EDITOR
                    return GetDefaultQualityDroid();
#endif
#if UNITY_IOS && !UNITY_EDITOR
                    return GetDefaultQualityIos();
#endif
                    return QualitySettings.names.Length;
                }
            }
        }

        int GetDefaultQualityDroid()
        {
            int result = 2;
            if (GetAndroidSDKLevel() < 26)
            {
                result--;
            }

            if (Screen.height * Screen.width < 1920 * 1080)
            {
                result--;
            }
            
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
            {
                result--;
            }

            if (SystemInfo.systemMemorySize < 1900)
                result--;

            return result<0?0:result;
        }

        //https://gamedev.stackexchange.com/questions/95342/is-there-a-way-to-check-android-version-running-the-game-using-unity3d
        public int GetAndroidSDKLevel()
        {
            var clazz = AndroidJNI.FindClass("android/os/Build$VERSION");
            var fieldID = AndroidJNI.GetStaticFieldID(clazz, "SDK_INT", "I");
            var sdkLevel = AndroidJNI.GetStaticIntField(clazz, fieldID);
            return sdkLevel;
        }

        int GetDefaultQualityIos()
        {
            if (IsLowEndDevice)
            {
                return 0;
            }
            if (IsMediumDevice)
            {
                return QualitySettings.names.Length / 2;
            }
            else
            {
                return QualitySettings.names.Length - 1;
            }
        }
        public void SetQualityByUserChoice(int qualityLevel)
        {
            IsQualityOverrided = true;
            SavedQualityLevel = qualityLevel;
            QualitySettings.SetQualityLevel(qualityLevel, true);
            UpdateQuality();
        }

        private static bool IsQualityOverrided
        {
            get { return PlayerPrefs.GetInt(IsQualityOverridedKey, 0) == 1; }

            set
            {
                PlayerPrefs.SetInt(IsQualityOverridedKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private static int SavedQualityLevel
        {
            get { return PlayerPrefs.GetInt(SavedQualityLevelKey); }
            set { PlayerPrefs.SetInt(SavedQualityLevelKey, value);}
        }

#if UNITY_IPHONE
        protected bool IsLowEndDevice
        {
            get
            {
                var result = (Application.platform == RuntimePlatform.IPhonePlayer &&
                    Array.IndexOf(SlowIphones, Device.generation) != -1);

                //Debug.Log("Application.platform: " + Application.platform + " iPhone.generation " + Device.generation + " result: " + result);
                return result;
            }
        }

        protected bool IsMediumDevice
        {
            get { return false; }
        }

        private static readonly UnityEngine.iOS.DeviceGeneration[] SlowIphones =
        {
            DeviceGeneration.iPad1Gen,
            DeviceGeneration.iPad2Gen, 
            DeviceGeneration.iPad3Gen,
            DeviceGeneration.iPad4Gen,
            DeviceGeneration.iPadMini1Gen,
            DeviceGeneration.iPadMini2Gen,
            DeviceGeneration.iPhone, 
            DeviceGeneration.iPhone3G, 
            DeviceGeneration.iPhone3GS,
            DeviceGeneration.iPhone4,
            DeviceGeneration.iPhone4S,
            DeviceGeneration.iPhone5,
            DeviceGeneration.iPhone5C,
            DeviceGeneration.iPodTouch1Gen,
            DeviceGeneration.iPodTouch2Gen,
            DeviceGeneration.iPodTouch3Gen,
            DeviceGeneration.iPodTouch4Gen,
            DeviceGeneration.iPodTouch5Gen
        };
#else
        protected virtual bool IsLowEndDevice
        {
            get { return false; // Application.platform == RuntimePlatform.Android;
            }
        }
        protected virtual bool IsMediumDevice
        {
            get { return Application.platform == RuntimePlatform.Android; }
        }
#endif
    }
}

