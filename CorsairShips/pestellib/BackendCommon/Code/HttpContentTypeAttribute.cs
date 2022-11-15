using System;

namespace BackendCommon.Code
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HttpContentTypeAttribute : Attribute
    {
        public string MimeType { get; set; }
    }
}
