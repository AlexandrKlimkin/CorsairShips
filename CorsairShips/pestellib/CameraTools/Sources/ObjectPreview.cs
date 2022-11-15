using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using UnityEngine.EventSystems;

public class ObjectPreview : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    private enum State
    {
        Idle,
        Rotating,
        Zooming
    }

    public Action OnStartRotation = () => { };
    public Action OnEndRotation = () => { };
    public Action OnObjectZoomed = () => { };

    [SerializeField] private Camera _camera;
	[SerializeField] private float _sensivityX = 0.2f;
	[SerializeField] private float _sensivityY = 0.1f;
	[SerializeField] private float _minDeltaAngleY = -15f;
	[SerializeField] private float _maxDeltaAngleY = 25f;
	[SerializeField] private float _freeDecceleration = 90f;
	[SerializeField] private float _holdDecceleration = 500f;
	[SerializeField] private float _decelerationSpeedMultipler = 0.06f;
	[SerializeField] private float _maxAcceleration = 360000f;
	[SerializeField] private float _maxSpeed = 1080f;
	[SerializeField] private float _moveTime = 0.8f;
	[SerializeField] protected float _cellSizeX = 2f;
	[SerializeField] private bool _FitCameraToWidth = false;
	[SerializeField] protected Transform _modelsRoot;
    [SerializeField] protected Transform _closeViewModelRoot;
    [SerializeField] private List<Transform> models = new List<Transform>();
    [SerializeField] private bool _freezePosition;

    private bool _inited;
    private Transform _currModel;
    private int _currIndex = 0;
    private int _prevIndex = 0;

    private Tweener _tweener;

    private float _speedX = 0f;
    private float _accelerateX = 0f;
    private float _angleX = 0f;
    private float _angleY = 0f;

    private float _sensX;
    private float _sensY;

    private State _state;

    public bool IsZoomed { get; private set; }

    public Camera Camera
    {
        get { return _camera; }
        set { _camera = value; }
    }

    protected virtual void Start()
	{
	    _sensX = _sensivityX;
	    _sensY = _sensivityY;
        
		if (_FitCameraToWidth)
			Camera.fieldOfView *= (16f / 9f) / (Camera.pixelWidth / Camera.pixelHeight);

		if (models.Count > _currIndex)
			_currModel = models[_currIndex];
	}

    private void OnDestroy()
    {
        KillTweener();
    }

    private void InitModels()
	{
        if (models.Count == 0)
        {
            return;
        }

        if (_inited)
        {
            return;
        }

        if (models.Count > _currIndex && _currIndex >= 0)
        {
            _currModel = models[_currIndex];
            _inited = true;
        }
	}

    protected virtual void Update()
	{
		if (Math.Abs(_speedX) > 0 && _currModel != null)
		{
			_angleY += _speedX * Time.unscaledDeltaTime;
			UpdateRotation();

            float deceleration = (_state == State.Rotating) ? _holdDecceleration : _freeDecceleration;

			_accelerateX = -(Mathf.Sign(_speedX) * Mathf.Min(deceleration + Mathf.Pow(Mathf.Abs(_speedX) * _decelerationSpeedMultipler, 2f), _maxAcceleration));
			_speedX += _accelerateX * Time.unscaledDeltaTime;
			_speedX = Mathf.Sign(_speedX) * Mathf.Min(Mathf.Abs(_speedX), _maxSpeed);

			if (Mathf.Sign(_speedX) == Mathf.Sign(_accelerateX))
				_speedX = 0;
		}
	}

    protected void UpdateRotation()
	{
		if (_currModel != null)
		{
			//Quaternion rotX = Quaternion.AngleAxis(_angleX, _camera.transform.right);
			//Quaternion rotY = Quaternion.AngleAxis(_angleY, Vector3.up);
            //_currModel.transform.localRotation = rotX * rotY;

		    foreach (var model in models)
		    {
                model.transform.localRotation = Quaternion.identity;
                model.transform.RotateAround(model.transform.position, Vector3.up, _angleY);
		    }
		}
	}

    [ContextMenu("Next")]
	public void ShowNext()
    {
        _prevIndex = _currIndex;
		_currIndex = Math.Min(_currIndex + 1, models.Count - 1);
		ShowModel();
	}

    [ContextMenu("Prev")]
	public void ShowPrev()
    {
        _prevIndex = _currIndex;
		_currIndex = Math.Max(_currIndex - 1, 0);
		ShowModel();
	}

    public void ShowIndex(int index)
    {
        if (index == _currIndex) return;

        _prevIndex = _currIndex;
        _currIndex = index;
        ShowModel();
    }

    private void ShowModel()
	{
        if (_currModel != null)
        {
            if (_currModel.parent == _closeViewModelRoot)
            {
                ParentCurModelTo(_modelsRoot, new Vector3(_cellSizeX * (models.IndexOf(_currModel)), 0, 0));
                IsZoomed = false;
            }
        }

		_currModel = models[_currIndex];
        var prevModel = models[_prevIndex];

        _speedX = 0;

        var offset = _prevIndex < _currIndex ? _cellSizeX : -_cellSizeX;

        _currModel.localPosition = new Vector3(offset, 0, 0);
        prevModel.localPosition = new Vector3(0, 0, 0);

        _currModel.DOLocalMoveX(0, _moveTime).SetEase(Ease.InOutCubic);
        prevModel.DOLocalMoveX(-offset, _moveTime).SetEase(Ease.InOutCubic);
	}

    private void KillTweener()
	{
		if (_tweener != null)
		{
			_tweener.Kill();
			_tweener = null;
		}
	}

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta;
        
        if (Math.Abs(delta.x) > 0)
        {
            _speedX = -delta.x * _sensX / Time.unscaledDeltaTime;
        }

        _angleX += delta.y * _sensY;
        if (_angleX > _maxDeltaAngleY) _angleX = _maxDeltaAngleY;
        if (_angleX < _minDeltaAngleY) _angleX = _minDeltaAngleY;
        UpdateRotation();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_state != State.Idle) return;
        
        _state = State.Rotating;
        OnStartRotation();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _state = State.Idle;
        OnEndRotation();
    }
    
    public void AddModel(Transform model)
    {
        if (models.Contains(model))
        {
            models.Remove(model);
        }

        models.Add(model);
        InitModels();
        if (!_freezePosition)
            model.transform.localPosition = new Vector3(_cellSizeX * (models.Count - 1), 0, 0);
    }
    
    public void ToggleZoom()
    {
        if (_state != State.Idle || _currModel == null || _closeViewModelRoot == null) return;

        if (_currModel.parent == _modelsRoot)
        {
            ParentCurModelTo(_closeViewModelRoot, Vector3.zero);
            IsZoomed = true;
        }
        else
        {
            ParentCurModelTo(_modelsRoot, new Vector3(_cellSizeX * (models.IndexOf(_currModel)), 0, 0));
            IsZoomed = false;
        }

        _state = State.Zooming;

        _angleY = _angleX = _speedX = 0;
    }

    private void ParentCurModelTo(Transform root, Vector3 destinationPos)
    {
        _currModel.parent = root;

        _tweener = _currModel.DOLocalMove(destinationPos, _moveTime)
            .SetEase(Ease.InOutQuart)
            .SetUpdate(UpdateType.Normal, true)
            .OnComplete(() => { _state = State.Idle; });

        _tweener = _currModel.DOLocalRotate(Vector3.zero, _moveTime)
            .SetEase(Ease.InOutQuart)
            .SetUpdate(UpdateType.Normal, true)
            .OnComplete(() => { _state = State.Idle; });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!eventData.IsPointerMoving() && !eventData.dragging)
        {
            OnObjectZoomed();
        }
    }
}