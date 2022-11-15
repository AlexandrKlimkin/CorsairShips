using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using PestelLib.Log.log4net.Appender;
using UnityEngine;

namespace PestelLib.Log.Sources.log4net
{
    public class ConfigureLogBehaviour : MonoBehaviour
    {
        public void Start()
        {
            DontDestroyOnLoad(gameObject);
            InitLogger();
        }

        private void InitLogger()
        {
            var unityLogger = new UnityAppender
            {
                Layout = new PatternLayout()
            };
            unityLogger.ActivateOptions();

            BasicConfigurator.Configure(unityLogger);
        }
    }
}
