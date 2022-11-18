namespace Game.SeaGameplay {
    public class LocalPlayerShipCreatedSignal {
        public Ship Ship;
        
        public LocalPlayerShipCreatedSignal(Ship ship) {
            Ship = ship;
        }
    }
}