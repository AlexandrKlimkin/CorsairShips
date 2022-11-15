using System;

namespace S
{
    public class ResponseException : Exception
    {
        public ResponseCode ResponseCode { get; private set; }
        public string DebugMessage { get; set; }

        public ResponseException(ResponseCode responseCode)
            :this(responseCode, null)
        {
        }

        public ResponseException(ResponseCode responseCode, string debugMessage)
        {
            ResponseCode = responseCode;
            DebugMessage = debugMessage;
        }
    }
}