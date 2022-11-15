using System;
using UnityEditor;
using UnityEngine;

namespace PestelLib.UtilsEditor
{
    [InitializeOnLoad]
    public class EditorWindowFocusUtility
    {
        public static event Action<bool> OnUnityEditorFocus = (focus) => { };
        private static bool _appFocused;

        static EditorWindowFocusUtility()
        {
            EditorApplication.update += Update;
        }
        
        private static void Update()
        {
            if (!_appFocused && UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                _appFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
                OnUnityEditorFocus(true);
            }
            else if (_appFocused && !UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                _appFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
                OnUnityEditorFocus(false);
            }
        }
    }
}