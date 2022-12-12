using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTPLib.Core.Utils;

namespace Game.SeaGameplay.Bounds {
    [ExecuteInEditMode]
    public class LevelBounds : MonoBehaviour {
        [SerializeField]
        private Renderer _Renderer;
        public float Radius;
        public float ViewScaleY;

        private void Awake() {
            ScaleView();
        }

        private void OnValidate() {
            ScaleView();
        }

        private void ScaleView() {
            _Renderer.transform.localScale = new Vector3(Radius * 2, ViewScaleY, Radius * 2);
        }

        public bool PositionInBounds(Vector3 pos) {
            var vector = pos - transform.position;
            var vectorXZ = vector.ToVector2XZ();
            return vectorXZ.sqrMagnitude < Radius * Radius;
        }

        public Vector3 GetClosestPointInBounds(Vector3 pos, float offset) {
            if (PositionInBounds(pos))
                return new Vector3(pos.x, 0, pos.z);
            var vector = pos - transform.position;
            var normalizedVector = vector.normalized;
            return transform.position + vector - normalizedVector * offset;
        }
    }
}
