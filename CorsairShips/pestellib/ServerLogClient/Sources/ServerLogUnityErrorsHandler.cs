using System;
using System.Collections;
using System.Text;
using BestHTTP;
using Newtonsoft.Json;
using PestelLib.ClientConfig;
using PestelLib.ServerLogProtocol;
using UnityEngine;
using UnityDI;
using UnityEngine.Assertions;

namespace PestelLib.ServerLogClient
{
    public class ServerLogUnityErrorsHandler : MonoBehaviour
    {
#pragma warning disable 649
        [Dependency] private Config _config;
#pragma warning restore 649

        private LogErrorsPack _logMessagesGroup = new LogErrorsPack();

        protected virtual IEnumerator Start()
        {
            ContainerHolder.Container.BuildUp(this);
            Application.logMessageReceived += HandleLog;
            Assert.IsNotNull(_config, "You have to register Config in ContainerHolder.Container to use ServerLogUnityErrorsHandler");

            _logMessagesGroup.Game = ServerLog.GameName;

            if (Application.isEditor) yield break; //we don't want to collect messages from editor

            while (true)
            {
                if (_logMessagesGroup.Errors.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(_logMessagesGroup);
                    _logMessagesGroup.Errors.Clear();

                    var request = new HTTPRequest(new Uri(_config.LogServer + "CountLogError.ashx"), HTTPMethods.Post);
                    request.RawData = Encoding.UTF8.GetBytes(json);
                    request.Send(); //we don't really care if it reaches the server or not, so we don't check results
                }

                yield return new WaitForSeconds(1);
            }
        }

        protected virtual void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception)
            {
                return;
            }

            _logMessagesGroup.Errors.Add(message);
        }
    }
}