using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.UtilsEditor.Sources
{
    public class FindFont
    {
        [MenuItem("PestelCrew/Find 'Arial' usages")]
        public static void FindArial()
        {
            var texts = Resources.FindObjectsOfTypeAll<Text>();
            foreach (var text in texts)
            {
                if (text.font.name.Contains("Arial"))
                {
                    Debug.Log(FullName(text.gameObject));
                }
            }
        }

        public static string FullName(GameObject go)
        {
            var fullName = go.name;
            var p = go.transform.parent;
            while (p != null)
            {
                fullName = p.name + "." + fullName;
                p = p.parent;
            }
            return fullName;
        }
    }
}
