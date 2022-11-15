using System;
using System.Text;
using PestelLib.ClientConfig;
using PestelLib.ServerClientUtils;
using UnityEngine;
using UnityDI;
using PestelLib.Utils;

namespace PestelLib.SendFeedback
{
    public class SendFeedbackUtility
    {
        [Dependency] private IPlayerIdProvider playerIdProvider;
        [Dependency] private SharedTime _sharedTime;
        [Dependency] private Config _config;

        public static Type OverrideFeedbackType = typeof(SendFeedbackUtility);

        private static SendFeedbackUtility _instance;

        private static SendFeedbackUtility Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (SendFeedbackUtility)Activator.CreateInstance(OverrideFeedbackType);
                    ContainerHolder.Container.BuildUp(_instance);
                }

                return _instance;
            }
        }

        protected internal void SendFeedbackInternal()
        {
            Application.OpenURL("mailto:" + _config.SupportEmail + 
                                "?subject=support request for " + Application.productName + 
                                "&body=" + FormatMessage());
        }

        private StringBuilder FormatMessage()
        {
            var message = new StringBuilder();
            message.Append("%0D%0A%0D%0A%0D%0A%0D%0A%0D%0A");
            message.Append("------DO NOT DELETE------%0D%0A");
            message.Append("UTC Time: " + _sharedTime.Now + "%0D%0A");
            message.Append("Product: " + Application.productName + "%0D%0A");
            message.Append("Version: " + Application.version + "%0D%0A");

            if (playerIdProvider != null)
            {
                message.AppendLine("Player id: " + playerIdProvider.PlayerId + "%0D%0A");
            }

            message.Append("Device: " + SystemInfo.deviceModel + "%0D%0A");
            message.Append("OS: " + SystemInfo.operatingSystem + " " + SystemInfo.operatingSystemFamily + (APKIntegrityChecker.Check() ? ".1" : ".0") + "%0D%0A");
            message.Append("DeviceID: " + SystemInfo.deviceUniqueIdentifier + "%0D%0A");
            message.Append("Social.localUser.authenticated: " + Social.localUser.authenticated + "%0D%0A");
            return message;
        }

        public static void SendFeedback()
        {
            Instance.SendFeedbackInternal();
        }
    }
}
