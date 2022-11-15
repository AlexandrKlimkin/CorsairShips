using UnityEngine;

namespace PestelLib.Utils
{ 
	public static class LogUtils
    {
	    public static void FrameLog(string msg)
	    {
	        Debug.Log(Time.frameCount + " >> " + msg);
	    }
	}
}