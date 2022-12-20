using UnityEngine;

namespace Menu.Camera {
    public interface ICameraTarget {
        Transform Transform { get; }
        Vector3 ViewPoint { get; }

        Vector3 GetProjectPoint(Vector3 forward);
    }
}