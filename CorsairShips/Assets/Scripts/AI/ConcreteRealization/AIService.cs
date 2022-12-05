using PestelLib.Utils;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.Services.ResourceLoader;

namespace Game.SeaGameplay.AI {
    public class AIService : Scheduler<ShipAIController>, ILoadableService, IUnloadableService {
        [Dependency]
        private readonly UnityEventsProvider _UnityEventProvider;
        [Dependency]
        private readonly IResourceLoaderService _ResourceLoader;

        protected override float ObjectsPerFrame => 20f;

        public void Load() {
            Play(_UnityEventProvider);
        }

        public void Unload() {
            Stop();
        }

        protected override void UpdateObject(ShipAIController target) {
            if(!target)
                return;
            if(target.Ship.Dead)
                return;
            target.UpdateBT();
        }

        public void MakeShipAI(Ship ship, ShipAIController aiController) {
            if(ship == null)
                return;
            if(aiController == null)
                return;
            var newController = Object.Instantiate(aiController, ship.transform);
            //aiController.transform.SetParent(unit.transform);
            newController.transform.localPosition = Vector3.zero;
            newController.transform.localRotation = Quaternion.identity;
            newController.Init();
        }
    }
}
