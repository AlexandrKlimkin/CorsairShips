using System.Collections.Generic;
using System.Collections;
using UnityEngine;


public abstract class Scheduler<S>
{

    protected virtual bool MaintainConstantLoadAmmount { get { return false; } }
    protected virtual float ObjectsPerFrame { get { return 1; } }

    private int RegistrySize
    {
        get
        {
            if(MaintainConstantLoadAmmount)
            {
                return _Registry.Count;
            }
            else
            {
                return _MaxRegistryCount;
            }
        }
    }

    private readonly List<S> _Registry = new List<S>();
    private int _MaxRegistryCount;
    private int _CurrentUpdateIndex;
    private float _AccumulatedObjectFraction;
    private MonoBehaviour _PlayTarget;
    private Coroutine _PlayCoroutine;

    protected bool _IsPlaying;

    protected void Play(MonoBehaviour playTarget)
    {
        if(_IsPlaying)
        {
            return;
        }
        _IsPlaying = true;
        _PlayTarget = playTarget;
        _PlayCoroutine = _PlayTarget.StartCoroutine(UpdateTask());
    }

    protected void Stop()
    {
        _IsPlaying = false;
        _PlayTarget.StopCoroutine(_PlayCoroutine);
        _PlayTarget = null;
    }

    protected virtual IEnumerator UpdateTask()
    {
        while(_IsPlaying)
        {
            if(_Registry.Count < 1)
                yield return null;

            _AccumulatedObjectFraction += Mathf.Max(0, ObjectsPerFrame);
            if(_AccumulatedObjectFraction >= 1)
            {
                var intPart = Mathf.FloorToInt(_AccumulatedObjectFraction);
                var fracPart = _AccumulatedObjectFraction - intPart;
                _AccumulatedObjectFraction = fracPart;

                for(int i = 0; i < intPart; i++)
                {
                    if(_CurrentUpdateIndex > RegistrySize - 1)
                        break;

                    if(_CurrentUpdateIndex < _Registry.Count)
                    {
                        UpdateObject(_Registry[_CurrentUpdateIndex]);
                    }
                    else
                    {
                        yield return null;
                    }

                    var nextIndex = _CurrentUpdateIndex + 1;
                    _CurrentUpdateIndex = nextIndex < RegistrySize ? nextIndex : 0;
                }
            }
            else
            {
                yield return null;
            }

            yield return null;
        }
    }

    protected abstract void UpdateObject(S target);

    public void SetMinRegistrySize(int size)
    {
        _MaxRegistryCount = size;
    }

    public void Register(S target)
    {
        if(_Registry.Contains(target))
            return;
        _Registry.Add(target);
        if(_Registry.Count > _MaxRegistryCount)
        {
            _MaxRegistryCount = _Registry.Count;
        }
    }

    public void Unregister(S target)
    {
        _Registry.Remove(target);
    }
}