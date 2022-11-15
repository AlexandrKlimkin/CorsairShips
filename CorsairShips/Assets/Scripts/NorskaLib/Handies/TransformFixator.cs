using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TransformFixator : MonoBehaviour
{
    public enum Modes
    {
        Custom,
        OnUpdate,
        OnFixedUpdate
    }

    [SerializeField] Transform target;

    //[SerializeField] bool fixatePosition;
    //[SerializeField] Vector3 worldPosition;
    //[SerializeField] bool fixateRotation;
    [SerializeField] Vector3 worldRotation;
    //[SerializeField] bool fixateScale;
    //[SerializeField] Vector3 worldScale;

    [Space]
    [SerializeField] Modes mode;

    void Update()
    {
#if UNITY_EDITOR
        if (mode != Modes.OnUpdate && !runInEditMode)
            return;
#else
        if (mode != Modes.OnUpdate)
            return;
#endif

        Fixate(target);
    }

    void FixedUpdate()
    {
        if (mode != Modes.OnFixedUpdate)
            return;

        Fixate(target);
    }

    private void Fixate(Transform target)
    {
        if (target == null)
            return;

        target.rotation = Quaternion.Euler(worldRotation);
    }
}

