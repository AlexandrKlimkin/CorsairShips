using Game.SeaGameplay;

namespace Menu.Hagar {
    public struct HangarShipChanged {
        public ShipModelController ShipModel;
        public HangarShipChanged(ShipModelController shipModel) {
            ShipModel = shipModel;
        }
    }
}