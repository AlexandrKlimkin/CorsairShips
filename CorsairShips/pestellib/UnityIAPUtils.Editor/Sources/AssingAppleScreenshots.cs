using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing;

namespace PestelLib.UnityIAPUtils.Editor.Sources
{
    public class AssingAppleScreenshots
    {
        private const string ApplesSreensKey = "AppleScreens";

        [MenuItem("PestelCrew/UnityIAP/Apple review screenshots: Enable")]
        static void TurnScreenshotToolOn()
        {
            EditorPrefs.SetBool(ApplesSreensKey, true);
        }

        [MenuItem("PestelCrew/UnityIAP/Apple review screenshots: Enable", true)]
        public static bool ValidateTurnScreenshotToolOn()
        {
            return !EditorPrefs.GetBool(ApplesSreensKey);
        }

        [MenuItem("PestelCrew/UnityIAP/Apple review screenshots: Disable")]
        static void TurnScreenshotToolOff()
        {
            EditorPrefs.SetBool(ApplesSreensKey, false);
        }

        [MenuItem("PestelCrew/UnityIAP/Apple review screenshots: Disable", true)]
        public static bool ValidateTurnScreenshotToolOff()
        {
            return EditorPrefs.GetBool(ApplesSreensKey);
        }

        [MenuItem("PestelCrew/UnityIAP/Assing apple review screenshots")]
        public static void AssignScreenshots()
        {
            var resource = Resources.Load<TextAsset>("IAPProductCatalog");
            var data = ProductCatalog.Deserialize(resource.text);
            var dataDict = (ProductCatalog)data;
            var list = dataDict.allProducts;

            foreach (var productCatalogItem in list)
            {
                var screenPath = Application.dataPath.Replace("/Assets", "/PremiumShopScreenshots/") + productCatalogItem.id + ".png";
                productCatalogItem.screenshotPath = screenPath;

                if (!File.Exists(screenPath))
                {
                    Debug.LogError("Can't find screenshot " + screenPath);
                }
                else
                {
                    Debug.Log("Assigned: " + screenPath);
                }
            }

            var result = ProductCatalog.Serialize(data);
            var fullPath = Application.dataPath.Replace("Assets", string.Empty) + AssetDatabase.GetAssetPath(resource);
            File.WriteAllText(fullPath, result);
        }
    }
}