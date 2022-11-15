using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using BackendCommon.Code;
using log4net;
using ServerShared;
using RequestContext = System.Web.Routing.RequestContext;

namespace Backend
{
    public class HttpHandlerToRequestHandler : IRouteHandler, IHttpHandler
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(HttpHandlerToRequestHandler));
        private IRequestHandler _handler;
        private Dictionary<string, string> _headers = new Dictionary<string, string>();

        public HttpHandlerToRequestHandler(IRequestHandler handler)
        {
            _handler = handler;
            foreach (var attr in _handler.GetType().CustomAttributes)
            {
                if(attr.AttributeType != typeof(HttpContentTypeAttribute))
                if(attr.NamedArguments == null)
                    continue;
                foreach (var argument in attr.NamedArguments)
                {
                    if (argument.MemberName == nameof(HttpContentTypeAttribute.MimeType))
                    {
                        _headers["Content-Type"] = (string) argument.TypedValue.Value;
                    }
                }
            }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                var data = context.Request.InputStream.ReadAll();
                var response = _handler.Process(data, new BackendCommon.Code.RequestContext() {RemoteAddr = context.Request.UserHostAddress}).Result;
                if (response.Length > 0)
                {
                    foreach (var header in _headers)
                    {
                        context.Response.Headers.Add(header.Key, header.Value);
                    }

                    context.Response.OutputStream.Write(response, 0, response.Length);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public bool IsReusable => true;
    }
}