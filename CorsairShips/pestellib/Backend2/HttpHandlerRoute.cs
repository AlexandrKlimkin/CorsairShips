using System;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;

namespace ServerLib
{
    public class HttpHandlerRoute : IRouteHandler
    {

        private String _VirtualPath = null;

        public HttpHandlerRoute(String virtualPath)
        {
            _VirtualPath = virtualPath;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            IHttpHandler httpHandler = (IHttpHandler)BuildManager.CreateInstanceFromVirtualPath(_VirtualPath, typeof(IHttpHandler));
            return httpHandler;
        }
    }
}