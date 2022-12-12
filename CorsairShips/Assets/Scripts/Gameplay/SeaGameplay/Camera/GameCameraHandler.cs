using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityDI;
using UnityEngine;
using UTPLib.Services.ResourceLoader;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.Camera {
    public class GameCameraHandler : MonoBehaviour {
        [Dependency]
        private readonly SignalBus _SignalBus;
        [Dependency]
        private readonly IResourceLoaderService _ResourceLoader;
        
        private SimpleFollowCamera _FollowCamera;
        private CameraOffsetConfig _ShipsOffsetConfig;

        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
            _FollowCamera = GetComponent<SimpleFollowCamera>();
            _SignalBus.Subscribe<LocalPlayerShipCreatedSignal>(OnLocalPlayerShipCreatedSignal, this);
            _ShipsOffsetConfig =
                _ResourceLoader.LoadResource<CameraOffsetConfig>(ResourcePath.Camera.GameCameraOffsetConfigPath);
        }

        private void OnDestroy() {
            _SignalBus.UnSubscribeFromAll(this);
        }

        private void OnLocalPlayerShipCreatedSignal(LocalPlayerShipCreatedSignal signal) {
            _FollowCamera.target = signal.Ship.transform;
            var offset = GetShipCameraOffset(signal.Ship.ShipDef.Id);
            _FollowCamera.offset = offset;
        }

        private Vector3 GetShipCameraOffset(string shipId) {
            var ext = _ShipsOffsetConfig.CameraExtensions.FirstOrDefault(_ => _.ShipId == shipId);
            if (ext == null)
                return _ShipsOffsetConfig.UsualOffset;
            return ext.Offest;
        }
    }
}
