using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Networking;

public class InternetChecker : MonoBehaviour
{
    public enum UpdateModes
    {
        Manual,
        OnUpdate,
        Periodic
    }

    private const string address = "https://www.google.com/";
    private UnityWebRequest request;
    private Coroutine pingRoutine;

    [SerializeField] bool hasTimeout = true;
    [EnableIf("timeout")]
    [SerializeField] int timeout = 5;

    [ShowInInspector]
    private bool isConnected = false;
    public bool IsConnected
    {
        get => isConnected;

        private set
        {
            if (value != isConnected)
                onConnectionStatusChanged.Invoke(value);
            isConnected = value;
        }
    }
    public Action<bool> onConnectionStatusChanged = (isConnected) => { };

    [SerializeField] UpdateModes updateMode;
    public UpdateModes UpdateMode => updateMode;
    [ShowIf("@this.updateMode == UpdateModes.Periodic")]
    [SerializeField] float updateInterval = 5f;
    public float UpdateTimer 
    { get; private set; }

    void Start()
    {
        UpdateTimer = updateInterval;
        Ping(hasTimeout, timeout);
    }

    void OnDestroy()
    {
        if (pingRoutine != null)
            StopCoroutine(pingRoutine);
        if (request != null)
            DisposeRequest();
    }

    void Update()
    {
        if (pingRoutine is null)
            switch (updateMode)
            {
                case UpdateModes.OnUpdate:
                    Ping(hasTimeout, timeout);
                    break;

                case UpdateModes.Periodic:
                    if (UpdateTimer > 0)
                        UpdateTimer -= Time.deltaTime;
                    else
                    {
                        UpdateTimer += updateInterval;
                        Ping(hasTimeout, timeout);
                    }
                    break;
            }

#if UNITY_EDITOR
        DebugUpdate();
#endif
    }

    private void DisposeRequest()
    {
        request.Dispose();
        request = null;
    }

    /// <summary>
    /// Sends request to update connection status.
    /// </summary>
    public Coroutine Ping(bool hasTimeout, int timeout)
    {
        if (pingRoutine != null)
            StopCoroutine(pingRoutine);
        if (request != null)
            DisposeRequest();

        return pingRoutine = StartCoroutine(PingRoutine(hasTimeout, timeout));
    }
    IEnumerator PingRoutine(bool hasTimeout, int timeout)
    {
        request = new UnityWebRequest(address);
        if (hasTimeout)
            request.timeout = timeout;

        yield return request.SendWebRequest();

        switch (request.result)
        {
            default:
            case UnityWebRequest.Result.InProgress:
            case UnityWebRequest.Result.Success:
                IsConnected = true;
                //Debug.Log($"Pinging '{address}' result '{request.result}' - IsConnected: {IsConnected}");
                break;
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
            case UnityWebRequest.Result.DataProcessingError:
                IsConnected = false;
                //Debug.Log($"Pinging '{address}' result '{request.result}' ({request.error}) - IsConnected: {IsConnected}");
                break;
        }
        DisposeRequest();
        pingRoutine = null;
    }

#if UNITY_EDITOR
    [Header("Debugging")]
    [DisableIf("@true")]
    public string PingStateView;
    [DisableIf("@true")]
    public float UpdateTimerView;

    private void DebugUpdate()
    {
        UpdateTimerView = UpdateTimer;

        if (request is null)
            PingStateView = "Idlding...";
        else
            PingStateView = $"Pinging {address} ... ({request.result})";
    }
#endif
}
