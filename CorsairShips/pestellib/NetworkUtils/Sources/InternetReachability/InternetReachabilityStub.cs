namespace PestelLib.NetworkUtils.Sources.InternetReachability
{
    public class InternetReachabilityStub : IInternetReachability
    {
        public bool HasInternet
        {
            get { return true; }
        }
        public bool WaitInternet(int timeout)
        {
            return true;
        }
    }
}
