using UnityDI;
using UTPLib.Services;

namespace Game.SeaGameplay.Points {
    public class PointsService : ILoadableService, IUnloadableService {
        [Dependency]
        private readonly IPointsCounter _PointsCounter;
        [Dependency]
        private readonly ILocalShipProvider _LocalShipProvider;
        
        public void Load() {
            
        }

        public void Unload() {
            
        }

        public int GetPointsCount(byte playerId) {
            return _PointsCounter.PointsDict[playerId];
        }

        public int GetLocalPlayerPointsCount() {
            return GetPointsCount(_LocalShipProvider.LocalShip.ShipData.ShipId);
        }
    }
}