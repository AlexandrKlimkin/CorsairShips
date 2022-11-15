using PestelLib.ServerClientUtils;
using S;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityDI;
using UnityEngine;

public class ConnectionChecker : MonoBehaviour
{
    [Dependency] private RequestQueue _requestQueue;
    [Dependency] private SharedTime _sharedTime;

    private RequestQueueItem _currentRequest;
    private Coroutine _failCoroutine;
    private List<Action> _onSuccess = new List<Action>();
    private List<Action> _onFail = new List<Action>();

    private void Awake()
    {
        ContainerHolder.Container.BuildUp(this);
    }

    public void CheckConnection(Action OnSuccess, Action OnFail, float timeout)
    {
        _onFail.Add(OnFail);
        _onSuccess.Add(OnSuccess);

        if (_currentRequest != null)
            return;

        _currentRequest = _requestQueue.SendRequest("SyncTime", new Request
        {
            SyncTime = new SyncTime()
        }, OnRequestSuccess);

        _failCoroutine = StartCoroutine(RequestFailCoroutine(timeout));
    }

    private IEnumerator RequestFailCoroutine(float timeout)
    {
        yield return new WaitForSeconds(timeout);

        foreach (var action in _onFail)
        {
            action();
        };
        _requestQueue.CancelRequest(_currentRequest);
        _onFail.Clear();
        _onSuccess.Clear();
        _failCoroutine = null;
        _currentRequest = null;
    }

    private void OnRequestSuccess(Response response, DataCollection dataCollection)
    {
        foreach (var action in _onSuccess)
        {
            action();
        }
        _onFail.Clear();
        _onSuccess.Clear();
        StopCoroutine(_failCoroutine);
        _failCoroutine = null;
        _currentRequest = null;
    }
}
