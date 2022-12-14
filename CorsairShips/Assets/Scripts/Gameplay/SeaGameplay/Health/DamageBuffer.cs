using System.Collections.Generic;
using System.Linq;
using UnityDI;
using UnityEngine;
using UTPLib.SignalBus;

namespace Game.Dmg {

    public struct DamageBufferData {
        public ClientDamage Damage;
        public float Time;
    }
    
    public class DamageBuffer : MonoBehaviour {
        [Dependency]
        private readonly SignalBus _SignalBus;

        public float SummaryBufferedDamage => _SummaryBufferedDamageDict.Sum(_ => _.Value);

        private Queue<DamageBufferData> _Buffer = new();
        private Dictionary<IDamageCaster, float> _SummaryBufferedDamageDict = new();
        private float _SafeTime;
        private IDamageable _Damageable;

        public void Initialize(IDamageable dmgbl, float safeTime) {
            ContainerHolder.Container.BuildUp(this);
            _SafeTime = safeTime;
            _Damageable = dmgbl;
            _Damageable.OnTakeDamage += DmgblOnTakeDamage;
        }

        private void DmgblOnTakeDamage(ClientDamage dmg) {
            if(dmg.Caster == null)
                return;
            AddToBuffer(new DamageBufferData {
                Damage = dmg,
                Time = Time.time,
            });
        }

        private void OnDestroy() {
            _Damageable.OnTakeDamage += DmgblOnTakeDamage;
        }

        private void AddToBuffer(DamageBufferData damageBufferData) {
            _Buffer.Enqueue(damageBufferData);
            if (_SummaryBufferedDamageDict.ContainsKey(damageBufferData.Damage.Caster))
                _SummaryBufferedDamageDict[damageBufferData.Damage.Caster] += damageBufferData.Damage.Damage.Amount;
            else
                _SummaryBufferedDamageDict.Add(damageBufferData.Damage.Caster, damageBufferData.Damage.Damage.Amount);
        }

        private void UpdateBuffer() {
            while (_Buffer.Count > 0 && _Buffer.Peek().Time + _SafeTime < Time.time) {
                var damageData = _Buffer.Dequeue();
                if (!_SummaryBufferedDamageDict.ContainsKey(damageData.Damage.Caster)) 
                    continue;
                _SummaryBufferedDamageDict[damageData.Damage.Caster] -= damageData.Damage.Damage.Amount;
                if (Mathf.Abs(_SummaryBufferedDamageDict[damageData.Damage.Caster]) < 0.01f) // float error
                    _SummaryBufferedDamageDict.Remove(damageData.Damage.Caster);
            }
        }

        public IDamageCaster TopBufferedDmgCaster() {
            var first = _SummaryBufferedDamageDict.FirstOrDefault();
            if (first.Key == null)
                return null;
            var topDmger = first.Key;
            var topDamage = first.Value;
            foreach (var summaryDamage in _SummaryBufferedDamageDict) {
                if (summaryDamage.Value > topDamage) {
                    topDamage = summaryDamage.Value;
                    topDmger = summaryDamage.Key;
                }
            }
            return topDmger;
        }

        public IEnumerable<IDamageCaster> GetCastersForLastTime(float time) {
            var now = Time.time;
            return _Buffer.Where(_ => now - _.Time > time).Select(_=>_.Damage.Caster);
        }
    }
}