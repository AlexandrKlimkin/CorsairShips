using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEngine.Purchasing;

namespace PestelLib.UnityIAPUtils.Editor.Sources
{
    public class LoadPricesFromGoogleCsv
    {
        //source: https://appstoreconnect.apple.com/WebObjects/iTunesConnect.woa/ra/ng/app/1437646483/pricingMatrix
        public static readonly decimal[] AppleUSDTiers =
        {
            0M, 0.99M, 1.99M, 2.99M, 3.99M, 4.99M, 5.99M, 6.99M, 7.99M, 8.99M, 9.99M, 10.99M, 11.99M, 12.99M, 13.99M, 14.99M, 15.99M, 16.99M, 17.99M, 18.99M, 19.99M, 20.99M, 21.99M, 22.99M, 23.99M, 24.99M, 25.99M, 26.99M, 27.99M, 28.99M, 29.99M, 30.99M, 31.99M, 32.99M, 33.99M, 34.99M, 35.99M, 36.99M, 37.99M, 38.99M, 39.99M, 40.99M, 41.99M, 42.99M, 43.99M, 44.99M, 45.99M, 46.99M, 47.99M, 48.99M, 49.99M, 54.99M, 59.99M, 64.99M, 69.99M, 74.99M, 79.99M, 84.99M, 89.99M, 94.99M, 99.99M, 109.99M, 119.99M, 124.99M, 129.99M, 139.99M, 149.99M, 159.99M, 169.99M, 174.99M, 179.99M, 189.99M, 199.99M, 209.99M, 219.99M, 229.99M, 239.99M, 249.99M, 299.99M, 349.99M, 399.99M, 449.99M, 499.99M, 599.99M, 699.99M, 799.99M, 899.99M, 999.99M
        };

        public static decimal GetAppleUSDPrice(decimal usdPrice, bool freeTierAllowed = false)
        {
            var minDistance = AppleUSDTiers.Min(n => Math.Abs(usdPrice - n));
            var closest = AppleUSDTiers.First(n => Math.Abs(usdPrice - n) == minDistance);

            if (!freeTierAllowed && closest == AppleUSDTiers[0])
            {
                closest = AppleUSDTiers[1];
            }
            return closest;
        }

        public static int GetAppleTier(decimal usdPrice)
        {
            for (var i = 0; i < AppleUSDTiers.Length; i++)
            {
                if ((decimal) AppleUSDTiers[i] == usdPrice)
                {
                    return i;
                }
            }

            Debug.LogError("Can't find tier for price: " + usdPrice);
            return AppleUSDTiers.Length - 1;
        }

        [MenuItem("PestelCrew/UnityIAP/Load price-title-desc from google csv")]
        public static void LoadPrices()
        {
            var path = EditorUtility.OpenFilePanel("Select google .csv from google play developer console", "", "csv");
            if (string.IsNullOrEmpty(path)) return;

            var googleData = new List<Dictionary<string, string>>();

            try
            {
                var csvText = File.ReadAllText(path);
                var csvRows = csvText.Split('\n');
                var fieldNames = csvRows[0].Split(',');

                //пропускается первая строка (названия столбцов) и последняя (пустая)
                for (var rowIndex = 1; rowIndex < csvRows.Length - 1; rowIndex++)
                {
                    var csvRow = csvRows[rowIndex];

                    var googleIapp = new Dictionary<string, string>();
                    var fieldValues = csvRow.Split(',');
                    for (var i = 0; i < fieldNames.Length && i < fieldValues.Length; i++)
                    {
                        googleIapp[fieldNames[i]] = fieldValues[i];
                    }

                    googleData.Add(googleIapp);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Something wrong with csv format." + e + " " + e.StackTrace);
                return;
            }

            var resource = Resources.Load<TextAsset>("IAPProductCatalog");
            var data = ProductCatalog.Deserialize(resource.text);
            var dataDict = (ProductCatalog)data;
            var list = dataDict.allProducts;

            foreach (var product in list)
            {
                var googleProduct = googleData.FirstOrDefault(x => x["Product ID"] == product.id);

                if (googleProduct == null)
                {
                    Debug.LogError("Can't find product with id " + product.id);
                    continue;
                }

                UpdateProduct(googleProduct, product);
            }

            foreach (var product in googleData)
            {
                string productId = product["Product ID"];
                if (dataDict.allProducts.ToArray().All(x => x.id != productId))
                {
                    var item = new ProductCatalogItem {id = productId};
                    var googleProduct = googleData.FirstOrDefault(x => x["Product ID"] == productId);
                    UpdateProduct(googleProduct, item);
                    dataDict.allProducts.Add(item);
                }
            }
            
            var result = ProductCatalog.Serialize(data);
            var fullPath = Application.dataPath.Replace("Assets", string.Empty) + AssetDatabase.GetAssetPath(resource);
            File.WriteAllText(fullPath, result);
        }

        private static void UpdateProduct(Dictionary<string, string> googleProduct, ProductCatalogItem product)
        {
            var priceCoeff = 1000000.0M;
            var priceStringData = googleProduct["Price"];
            var priceData = priceStringData.Split(';');
            var googlePrices = new Dictionary<string, string>(); //currency:value
            for (var i = 0; i < priceData.Length; i += 2)
            {
                googlePrices[priceData[i].Trim()] = priceData[i + 1].Trim();
            }

            if (googlePrices.ContainsKey("US"))
            {
                var priceValue = (decimal.Parse(googlePrices["US"]) / priceCoeff);
                priceValue = GetAppleUSDPrice(priceValue);
                Debug.Log(product.id + ": " + priceValue);

                product.googlePrice.value = priceValue;
                product.applePriceTier = GetAppleTier(priceValue);
            }
            else
            {
                Debug.LogError("Can't find US price in " + priceStringData + " for product " + product.id);
            }

            var description = googleProduct["Locale; Title; Description"];
            var descList = description.Split(';');

            product.translatedDescriptions.Clear();

            for (var i = 0; i <= descList.Length - 3; i += 3)
            {
                var locale = (TranslationLocale) Enum.Parse(typeof(TranslationLocale), descList[i + 0]);
                var title = descList[i + 1];
                var desc = descList[i + 2];

                var productDesc = new LocalizedProductDescription
                {
                    Description = desc,
                    googleLocale = locale,
                    Title = title
                };

                if (i == 0)
                {
                    product.defaultDescription = productDesc;
                }
                else
                {
                    product.translatedDescriptions.Add(productDesc);
                }
            }
        }
    }
}