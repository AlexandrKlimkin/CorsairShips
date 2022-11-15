using System;
using System.Diagnostics;

namespace PestelLib.Serialization
{
    [Conditional("UNITY_EDITOR")]
    public class GoogleSpreadsheetUriAttribute : Attribute
    {
        public string Uri;
        public string CopyTo;

        public GoogleSpreadsheetUriAttribute(string uri)
        {
            Uri = uri;
        }

        public GoogleSpreadsheetUriAttribute(string uri, string copyTo)
        {
            Uri = uri;
            CopyTo = copyTo;
        }
    }
}