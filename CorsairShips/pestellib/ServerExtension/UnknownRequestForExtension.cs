using System;

namespace ServerExtension
{
    public class UnknownRequestForExtension : Exception
    {
        public UnknownRequestForExtension(string message) : base(message)
        {
        }
    }
}