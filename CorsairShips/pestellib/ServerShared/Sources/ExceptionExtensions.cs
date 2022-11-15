using System;

namespace ServerShared
{
    public static class ExceptionExtensions
    {
        public static string Flatten(this Exception e)
        {
            if (e == null)
                return string.Empty;
            var message = e.ToString();
            return message + Flatten(e.InnerException);
        }
    }
}
