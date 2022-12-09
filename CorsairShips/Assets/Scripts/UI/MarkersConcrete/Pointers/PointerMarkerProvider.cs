using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Game.SeaGameplay;
using Sirenix.OdinInspector;
using UnityDI;
using UnityEngine;

namespace UI.Markers.Pointers {
    public class PointerMarkerProvider : MarkerProvider<PointerMarkerWidget, PointerMarkerData> {

        [Dependency]
        protected readonly ILocalShipProvider _LocalShipProvider;

        public bool ShowPreview;
        [ShowIf("@ShowPreview")]
        public Sprite PreviewSprite;
        public bool UseDistanceFromHero;
        [ShowIf("@UseDistanceFromHero")]
        public float DistanceFromHeroRequired;
        [Range(0f, 300f)]
        public float BordersOffset;
        public bool ShowDistance;
        public Color BGColor;
        public Color DistanceTextColor;
        
        protected virtual void Start() {
            ContainerHolder.Container.BuildUp(GetType(),this);
        }

        public override bool GetVisibility() {
            var distCheck = !UseDistanceFromHero || Vector3.SqrMagnitude(_LocalShipProvider.LocalShip.transform.position - transform.position) >
                DistanceFromHeroRequired * DistanceFromHeroRequired;
            return _LocalShipProvider?.LocalShip != null && distCheck;
        }

        protected override void RefreshData(PointerMarkerData data) {
            base.RefreshData(data);
            data.ShowPreview = ShowPreview;
            data.BordersOffset = BordersOffset;
            data.ShowDistance = ShowDistance;
            data.IconSprite = PreviewSprite;
            data.BGColor = BGColor;
            data.DistanceTextColor = DistanceTextColor;
            if(ShowDistance && _LocalShipProvider.LocalShip)
                data.Distance = Vector3.Distance(_LocalShipProvider.LocalShip.transform.position, transform.position);
        }
    }
}
