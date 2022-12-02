using System.Collections.Generic;
using System.Linq;
using UTPLib.Services;

namespace Game.Dmg {
    public class DamageService : ILoadableService, IUnloadableService {

        private List<IDamageable> _Damageables = new List<IDamageable>();
        private List<IDamageCaster> _DamageCasters = new List<IDamageCaster>();

        public void Load() {
            
        }

        public void Unload() {
            
        }

        public void RegisterDamageable(IDamageable damageable) {
            if(!_Damageables.Contains(damageable))
                _Damageables.Add(damageable);
        }
        
        public void RegisterCaster(IDamageCaster caster) {
            if(!_DamageCasters.Contains(caster))
                _DamageCasters.Add(caster);
        }
        
        public void UnregisterDamageable(IDamageable damageable) {
            if(_Damageables.Contains(damageable))
                _Damageables.Remove(damageable);
        }
        
        public void UnregisterCaster(IDamageCaster caster) {
            if(_DamageCasters.Contains(caster))
                _DamageCasters.Remove(caster);
        }

        public void ApplyDamage(Damage damage) {
            var receiver = _Damageables.FirstOrDefault(_ => _.DamageableId == damage.ReceiverId);
            if(receiver == null)
                return;
            var caster = _DamageCasters.FirstOrDefault(_ => _.DamageCasterId == damage.CasterId);

            var clientDamage = new ClientDamage {
                Damage = damage,
                Caster = caster,
                Receiver = receiver,
            };
            receiver.ApplyDamage(clientDamage);
        }
    }
}