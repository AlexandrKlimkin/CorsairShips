using UnityEngine;
using System.Collections;

namespace PestelLib.UI
{ 
	public class StateText : MonoBehaviour
	{
	    [SerializeField] private Text3dSpawner _widgetSpawner = null;

	    public void ShowMessage(string text, float time, float timescale)
	    {
            StartCoroutine(ShowMessageRoutine(text, time));
	    }

        IEnumerator ShowMessageRoutine(string text, float time)
	    {
	        _widgetSpawner.Text = text;
	        yield return new WaitForSeconds(time);
	        _widgetSpawner.Text = "";
	    }
	}
}