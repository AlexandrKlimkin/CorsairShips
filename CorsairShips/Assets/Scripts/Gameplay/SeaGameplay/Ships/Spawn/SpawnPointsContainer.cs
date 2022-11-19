using System;
using System.Collections;
using System.Collections.Generic;
using PestelLib.SharedLogic.Extentions;
using UnityDI;
using UnityEngine;

namespace Game.SeaGameplay.Spawn {
    public class SpawnPointsContainer : MonoBehaviour {
        [SerializeField]
        private List<SpawnPoint> _SpawnPoints;

        public IReadOnlyList<SpawnPoint> SpawnPoints => _SpawnPoints;

        private void Awake() {
            ContainerHolder.Container.RegisterInstance(this);
        }

        private void OnDestroy() {
            ContainerHolder.Container.UnregisterInstance(this);
        }
    }
}
