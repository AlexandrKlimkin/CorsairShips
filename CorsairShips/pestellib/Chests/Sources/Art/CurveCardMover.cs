using UnityEngine;

public class CurveCardMover : MonoBehaviour
{  
	public RectTransform _RectTransform;
	public AnimationCurve _Curve;
	public float _Offset = 128f;
	public float _TimeScale = 4f;

    private float _startTimestamp;

    void OnEnable()
    {
        _startTimestamp = Time.time;
    }

    float Lifetime
    {
        get
        {
            return Time.time - _startTimestamp;
        }
    }
	
	// Update is called once per frame
	void Update ()
	{
        _RectTransform.anchoredPosition = new Vector2(_Curve.Evaluate((Lifetime) * 2) * _Offset, _RectTransform.anchoredPosition.y);
    }
}
