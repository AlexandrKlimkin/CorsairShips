using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Backend.Code.Modules.PlayerProfile;
using log4net;
using MessagePack;
using PestelLib.ServerShared;
using S;
using ServerShared;
using UnityDI;

namespace Backend.service
{
    /// <summary>
    /// Summary description for ProfileService
    /// </summary>
    public class ProfileService : IHttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProfileService));

        public void ProcessRequest(HttpContext context)
        {
            var _handler = ContainerHolder.Container.Resolve<PlayerProfileApiCallHandler>();
            if (_handler == null)
            {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = 501;
                context.Response.Write("No handler");
            }

            try
            {
                Process(context);
            }
            catch (Exception e)
            {
                Log.Error(e);
                context.Response.StatusCode = 500;
            }
        }

        private void Process(HttpContext context)
        {
            var data = context.Request.InputStream.ReadAll();
            var request = MessagePackSerializer.Deserialize<Request>(data);
            var serverRequest = new ServerRequest
            {
                Request = request
            };
            var module = new PlayerProfileApiModule();
            var response = module.ProcessCommand(serverRequest);
            context.Response.ContentType = "application/octet-stream";
            context.Response.BinaryWrite(response.Data);
        }

        public bool IsReusable => true;
    }
}