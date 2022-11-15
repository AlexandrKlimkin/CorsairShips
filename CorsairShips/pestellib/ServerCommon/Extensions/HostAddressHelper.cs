
namespace PestelLib.ServerCommon
{
    public static class HostAddressHelper
    {
        public static (string host, int port) GetAddressPort(string str)
        {
            var idx = str.IndexOf(":");
            return (str.Substring(0, idx), int.Parse(str.Substring(idx + 1)));
        }
    }
}
