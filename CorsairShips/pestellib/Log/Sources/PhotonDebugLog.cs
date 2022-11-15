using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace PestelLib.Log
{
    public class PhotonDebugLog
    {
        public const string CONNECTION = "PhotonConnection";
        [Conditional("PHOTON_DEBUG")]
        public static void Log(string tag, string message)
        {
            Debug.Log(string.Format("{0}: {1}", tag, message));
        }
    }
}
