using System;
using UnityEngine;

namespace PestelLib.SharedLogicClient
{ 
    [Obsolete("Just delete it")]
	public class CommandProcessorEventHandler : IDisposable
	{
	    public void Dispose(){}

        public static string JsonSavePath
        {
            get
            {
                return Application.persistentDataPath + "/saveRealtime.json";
            }
        }
    }
}