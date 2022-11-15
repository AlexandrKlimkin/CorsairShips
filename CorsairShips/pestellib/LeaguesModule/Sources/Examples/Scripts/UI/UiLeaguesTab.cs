using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiLeaguesTab : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField]
    private Image _icon;
    [SerializeField]
    private Text _name;
#pragma warning restore 649

    public void SetData(Sprite icon, string name)
    {
        _icon.sprite = icon;
        _name.text = name;
    }

    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
