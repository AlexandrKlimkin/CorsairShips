using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PestelLib.NetworkUtils
{
    public interface IInternetReachability
    {
        bool HasInternet { get; }
        bool WaitInternet(int timeout);
    }
}
