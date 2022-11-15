using PestelLib.UI;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class PopUpBackgroundAnim : CanvasGroupFader
{
    private BlurOptimized _blur;

    [SerializeField]
    private float _blurCoeff = 4;

    void Start() 
    {
        var canvas = transform.root.GetComponent<Canvas>();
        var cam = canvas.worldCamera;
        _blur = cam.GetComponent<BlurOptimized>();
        if (_blur == null)
        {
            _blur = cam.gameObject.AddComponent<BlurOptimized>();
        }
        _blur.enabled = false;
    }

    protected override void SetPopupBackgroundState(bool b)
    {
        base.SetPopupBackgroundState(b);
        _blur.enabled = b;
    }

    float _currentValue;
    protected override float CurrentAlpha
    {
        get
        {
            return _currentValue;
        }
        set
        {
            _currentValue = value;

            if (_blur == null) return;

            if (value > 0.5f) {
                _blur.blurSize = value * _blurCoeff;
                _blur.blurIterations = 2;
            } else {
                _blur.blurSize = value * 2 * _blurCoeff;
                _blur.blurIterations = 1;
            }
        }
    }
}
