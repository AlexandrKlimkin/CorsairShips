using System.Linq;
using Game.Dmg;
using Game.SeaGameplay.AI.Data;
using Game.SeaGameplay.AI.Tasks;
using Tools.BehaviourTree;
using UnityEditor;
using UnityEngine;

namespace Game.SeaGameplay.AI.Tasks {
	public class FindTargetTask : ShipTask {

		protected BBTargetData _AttackTargetData;
		protected FindTargetParameters _Parameters;

		private float _LastTargetFindTime;
		private float _LastTargetSeenTime;
		private float _TargetMissTime;

		private float SqrRadius => _Parameters.Radius * _Parameters.Radius;
		
		public override void Init() {
			base.Init();
			_AttackTargetData = Blackboard.Get<BBTargetData>();
			_Parameters = Blackboard.Get<FindTargetParameters>();
			IsUpdated = true;
			IsGizmosUpdated = true;
			// if(_Parameters.AggroIfTakeDamage)
			// 	Unit.Events.onTakesDamage += OnTakesDamage;
		}

		// public override void Dispose() {
		// 	base.Dispose();
		// 	// Unit.Events.onTakesDamage -= OnTakesDamage;
		// }

		// private void OnTakesDamage(Ship ship, ClientDamage dmg) {
		// 	if(Ship != ship)
		// 		return;
		// 	if (dmg.Caster is Ship caster) {
		// 		if (_AttackTargetData.Target == null)
		// 			_AttackTargetData.Target = caster;
		// 	}
		// }

		public override TaskStatus Run() {
			if (_AttackTargetData.Target != null && !_AttackTargetData.Target.IsAlive)
				ClearTarget();

			if (_AttackTargetData.Target == null || !_AttackTargetData.Target.IsAlive) {
				if (Time.time - _LastTargetFindTime > _Parameters.FindTargetPeriod) {
					var target = GetTargetByWeights();
					SetTarget(target);
				}
			}
			else {
				if (Time.time - _LastTargetFindTime > _Parameters.RefreshTargetPeriod) {
					var target = GetTargetByWeights();
					if(target != null)
						SetTarget(target);
				}
				var targetInRange = CheckTargetInRange();
				if (targetInRange) {
					_LastTargetSeenTime = Time.time;
				}
				else {
					if (Time.time - _LastTargetSeenTime > _Parameters.TryFindMissedTargetTime) {
						ClearTarget();
					}
				}
			}
			return GetTaskStatus();
		}

		protected virtual TaskStatus GetTaskStatus() {
			return _AttackTargetData.Target == null ? TaskStatus.Failure : TaskStatus.Success;
		}

		protected virtual void ClearTarget() {
			// if(_AttackTargetData.Target != null)
			// 	UnitAIEvents.OnUnitLostTarget?.Invoke(Unit, _AttackTargetData.Target);
			_AttackTargetData.Target = null;
			_LastTargetSeenTime = 0;
		}

		private void SetTarget(Ship target) {
			// var same = _AttackTargetData.Target == target;
			_AttackTargetData.Target = target;
			// if(_AttackTargetData.Target != null && !same)
			// 	UnitAIEvents.OnUnitFindTarget?.Invoke(Ship, _AttackTargetData.Target);
			if(_AttackTargetData.Target != null)
				_LastTargetSeenTime = Time.time;
		}

		private Ship GetTargetByWeights() {
			//ToDo: Teams
			var enemies = Ship.Ships.Where(_=> _ != Ship && _.IsAlive).ToList();

			Ship targetEnemy = null;
			var maxWeight = float.MinValue;
			
			foreach (var enemy in enemies) {
				var sqrDistToCharacter = Vector3.SqrMagnitude(Ship.Position - enemy.Position);
				if (SqrRadius > sqrDistToCharacter) {
					
					var weight = 0f;
					
					var distToCharacter = Mathf.Abs(Mathf.Sqrt(sqrDistToCharacter));
					var distWeight = (1 - distToCharacter / _Parameters.Radius) * _Parameters.DistanceWeight;
					weight += distWeight;

					// if (Unit.Owner != null) {
					// 	var attitude = Unit.Owner.GetAttitude(enemy.Owner);
					// 	var attitudeWeight = attitude * _Parameters.AttitudeWeight;
					// 	//enemies attitude < 0
					// 	weight -= attitudeWeight;
					// }

					if (weight > maxWeight) {
						maxWeight = weight;
						targetEnemy = enemy;
					}
				}
			}
			_LastTargetFindTime = Time.time;
			return targetEnemy;
		}
		
		private bool CheckTargetInRange() {
			if (_AttackTargetData.Target == null)
				return false;
			var sqrDistToCharacter = Vector3.SqrMagnitude(Ship.Position - _AttackTargetData.Target.Position);
			if (sqrDistToCharacter > SqrRadius)
				return false;
			return true;
		}

		protected override void OnDrawGizmos() {
#if UNITY_EDITOR
			base.OnDrawGizmos();
			if(_AttackTargetData.Target == null)
				Gizmos.color = Color.yellow;
			else
				Gizmos.color = Color.red;
			Handles.DrawWireDisc(Ship.Position, Vector3.up, _Parameters.Radius);
			if (_AttackTargetData.Target != null) {
				Gizmos.DrawLine(Ship.Position + Vector3.up, _AttackTargetData.Target.Position + Vector3.up);
			}
#endif
		}
	}
}