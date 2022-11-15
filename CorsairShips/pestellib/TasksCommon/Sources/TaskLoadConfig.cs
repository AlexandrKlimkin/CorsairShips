using System;
using BestHTTP;
using PestelLib.ClientConfig;
using PestelLib.TaskQueueLib;
using PestelLib.Utils;
using UnityDI;
using UnityEngine;

namespace PestelLib.TasksCommon
{
    public class TaskLoadConfig : Task
    {
        [Dependency] private Config _config;
        [Dependency] private UpdateProvider _updateProvider;

        private HTTPRequest _request;

        public override void Run()
        {
            ContainerHolder.Container.BuildUp(this);
            if (Application.isEditor || Debug.isDebugBuild)
            {
                OnComplete(this);
                return;
            }

            _updateProvider.OnUpdate += UpdateProviderOnOnUpdate;

            var uriString = _config.ConfigOverrideBaseURL + Application.version + ".json";
            Debug.Log("Request config: " + uriString);
            _request = new HTTPRequest(new Uri(uriString), HTTPMethods.Get);
            _request.DisableCache = true;
            _request.Send();
        }

        private void UpdateProviderOnOnUpdate()
        {
            if (_request.Exception != null)
            {
                Debug.Log("Can't load config from server - using default");
                Finish();
                return;
            }

            if (_request.State != HTTPRequestStates.Finished)
            {
                return;
            }

            var configText = _request.Response.DataAsText;
            try
            {
                _config.MergeWith(configText);
            }
            catch (Exception e)
            {
                Debug.Log("Unable to merge config. Using default.");
            }

            Finish();
        }

        private void Finish()
        {
            OnComplete(this);
            _updateProvider.OnUpdate -= UpdateProviderOnOnUpdate;
        }
    }
}