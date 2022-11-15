using UnityEngine;
using UnityEngine.UI;

public class GenericStatusMessage : MonoBehaviour
{
    [SerializeField] private Text _text;

    public string Message
    {
        get { return _text.text; }

        set { _text.text = value; }
    }
}
