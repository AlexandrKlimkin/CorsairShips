using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TransformReplicator : MonoBehaviour
{
    public enum Modes
    {
        Custom,
        OnUpdate,
        OnFixedUpdate
    }

    [SerializeField] Transform target;

    [SerializeField] bool replicatePosition;
    [SerializeField] bool replicateRotation;
    [SerializeField] bool replicateScale;

    [Space]
    [SerializeField] Modes mode;

//     void Update()
//     {
// #if UNITY_EDITOR
//         if (mode != Modes.OnUpdate && !runInEditMode)
//             return;
// #else
//             if (mode != Modes.OnUpdate)
//                 return;
// #endif
//
//         Replicate(target);
//     }
//
//     void FixedUpdate()
//     {
//         if (mode != Modes.OnFixedUpdate)
//             return;
//
//         Replicate(target);
//     }

    void Replicate(Transform target)
    {
        if (target == null)
            return;

        if (replicatePosition)
            this.transform.position = target.position;
        if (replicateRotation)
            this.transform.rotation = target.rotation;
        if (replicateScale)
            this.transform.localScale = target.localScale;
    }
}
