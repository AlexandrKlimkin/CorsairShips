using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GlobalConflictPlayer : MonoBehaviour
{

    public string PlayerId;
    public bool Active;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if (string.IsNullOrEmpty(PlayerId))
	    {
	        PlayerId = Guid.NewGuid().ToString();
	    }

	    var players = transform.parent.gameObject.GetComponentsInChildren<GlobalConflictPlayer>();
	    var ids = new List<string>();
	    foreach (var player in players)
	    {
	        if (ids.Contains(player.PlayerId))
	        {
                Debug.Log(string.Format("Player with id {0} already exists. Generating new id.", player.PlayerId));
	            player.PlayerId = Guid.NewGuid().ToString();
	        }
	        ids.Add(player.PlayerId);
        }
    }
}
