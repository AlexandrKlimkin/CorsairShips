namespace Game.SeaGameplay {
    public struct ShipCreatedSignal {
        public Ship Ship;
        public float Time;
        
        public ShipCreatedSignal(Ship ship, float time) {
            Ship = ship;
            Time = time;
        }
    }
}