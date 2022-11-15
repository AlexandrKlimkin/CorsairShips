using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing;

namespace DesertWars.Assets.Plugins.LibsSymlinks.Editor.UnityIAPUtils.Editor
{
    public static class AppleCheckTitleLength
    {
        [MenuItem("PestelCrew/UnityIAP/Check title length (fix error ITMS-4134)")]
        public static void CheckTitleLength()
        {
            var resource = Resources.Load<TextAsset>("IAPProductCatalog");
            var data = ProductCatalog.Deserialize(resource.text);
            var dataDict = (ProductCatalog)data;
            var list = dataDict.allProducts;

            foreach (var productCatalogItem in list)
            {
                var length = productCatalogItem.defaultDescription.Title.Length;
                if (length > 30)
                {
                    Debug.LogError(productCatalogItem.id + " has too long name: " + length + productCatalogItem.defaultDescription.Title);
                }

                foreach (var localizedProductDescription in productCatalogItem.translatedDescriptions)
                {
                    length = localizedProductDescription.Title.Length;
                    if (length > 30)
                    {
                        Debug.LogError(productCatalogItem.id + " has too long name: " + length + " " + localizedProductDescription.Title);
                    }
                }
            }
        }
    }
}
