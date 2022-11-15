using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
using Backend;
using ServerLib;
using log4net;
using PestelLib.ServerShared;
using BackendCommon;
using BackendCommon.Code;
using BackendCommon.Code.IapValidator;
using PestelLib.ServerCommon;

namespace Server
{
    public class Global : System.Web.HttpApplication
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Global));

        public static string LogFolder { get; private set; }

        protected void Application_Start(object sender, EventArgs e)
        {
            RuntimeSettings.AppRoot = HostingEnvironment.ApplicationHost.GetPhysicalPath();
            var logSubfolder = HostingEnvironment.ApplicationHost.GetVirtualPath().Replace('/', '_');
            Log.SetLogSubfolder(logSubfolder);

            if (!BackendInitializer.Init())
            {
                throw new Exception("Backend init failed");
            }

            log.Info("Init done; target framework = " + HttpRuntime.TargetFramework);

            var validatorApi = BackendInitializer.ServiceProvider.GetService(typeof(IIapValidator)) as IIapValidator;

            log.Debug($"DIR {RuntimeSettings.AppRoot}");
            String handlerPath = "~/Main.ashx";
            RouteTable.Routes.Add(new Route("api/{upload}", new HttpHandlerRoute(handlerPath)));
            RouteTable.Routes.Add(new Route("service/mainhandler", new HttpHandlerToRequestHandler(new MainHandlerInternal())));
            if (validatorApi != null)
                RouteTable.Routes.Add(new Route("validateReceipt", new HttpHandlerToRequestHandler(new IapValidatorRouteHandler(validatorApi, false))));
            RouteTable.Routes.Add(new Route("notifyMatchEnd", new HttpHandlerToRequestHandler(new MatchEndHandler())));
        }

        
    }
}