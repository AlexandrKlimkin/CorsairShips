using System;
using System.Collections;
using System.Collections.Generic;
using UnityDI;
using UnityEngine;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.Camera {
    public class GameCameraHandler : MonoBehaviour {
        [Dependency]
        private readonly SignalBus _SignalBus;
        
        private SimpleFollowCamera _FollowCamera;

        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
            _FollowCamera = GetComponent<SimpleFollowCamera>();
            _SignalBus.Subscribe<LocalPlayerShipCreatedSignal>(OnLocalPlayerShipCreatedSignal, this);
        }

        private void OnDestroy() {
            _SignalBus.UnSubscribeFromAll(this);
        }

        private void OnLocalPlayerShipCreatedSignal(LocalPlayerShipCreatedSignal signal) {
            _FollowCamera.target = signal.Ship.transform;
        }
    }
}
