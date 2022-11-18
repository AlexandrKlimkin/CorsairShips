namespace Game.SeaGameplay {
    public struct ShipCreatedSignal {
        public Ship Ship;
        
        public ShipCreatedSignal(Ship ship) {
            Ship = ship;
        }
    }
}