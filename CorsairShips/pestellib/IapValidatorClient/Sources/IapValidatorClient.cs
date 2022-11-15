using System;
using System.Collections.Generic;
using System.Text;
using PestelLib.ServerShared;
using UnityEngine.Purchasing;
using Newtonsoft.Json;
using BestHTTP;
using UnityEngine;

namespace IapValidator
{
    public class IapValidatorClient
    {
        private readonly Uri _url;

        public IapValidatorClient(string url)
        {
            _url = new Uri(url);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="callback">
        /// if IapValidateResult is null then some error had happend, see logs
        /// </param>
        public void ValidateReceipt(Product product, Action<Product, IapValidateResult> callback)
        {
            var receiptData = GetReceiptData(product.receipt);
            var request = new HTTPRequest(_url, HTTPMethods.Post);
            IapValidateQuery query = null;

#if UNITY_ANDROID
            query = new IapValidateQuery()
            {
                Platform = "android",
                Receipt = Convert.ToBase64String(Encoding.UTF8.GetBytes(receiptData))
            };
#elif UNITY_IOS
            query = new IapValidateQuery()
            {
                Platform = "ios",
                Receipt = receiptData
            };
#endif
            var queryJson = JsonConvert.SerializeObject(query);
            request.RawData = Encoding.UTF8.GetBytes(queryJson);
            request.Callback = (originalRequest, response) => OnRequestFinished(originalRequest, response, product, callback);
            request.Send();
        }

        private static void OnRequestFinished(HTTPRequest originalRequest, HTTPResponse response, Product product, Action<Product, IapValidateResult> callback)
        {
			if (callback == null) {
				Debug.Log ("Error: callback is null");
				return;
			}

			if (product == null) {
				Debug.Log ("Error: product is null");
				if (callback != null)
					callback(product, null);
				return;
			}

			if (response == null) {
				Debug.Log ("Error: responseIsNull");
				if (callback != null)
					callback(product, null);
				return;
			}

            if (response.StatusCode != 200)
            {
                Debug.LogError("IAP validator failed with status: " + response.StatusCode);
                if (callback != null)
                    callback(product, null);
				return;
            }

            try
            {
                var result = JsonConvert.DeserializeObject<IapValidateResult>(response.DataAsText);
                if (callback != null)
                    callback(product, result);
				return;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                if (callback != null)
                    callback(product, null);
            }
        }

        private string GetReceiptData(string receipt)
        {
            var wrapper = JsonConvert.DeserializeObject<Dictionary<string, string>>(receipt);
            if (wrapper == null)
                return null;

            return wrapper["Payload"];
        }
    }
}
