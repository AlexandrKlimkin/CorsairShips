using System.Collections.Generic;
using Game.SeaGameplay.AI.Data;
using Game.SeaGameplay.AI.Tasks;
using Tools.BehaviourTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UTPLib.Core.Utils;

namespace Game.SeaGameplay.AI.Tasks {
	public class MoveToTargetPointTask : ShipTask {

		private BBMovementData _MovementData;

		private Vector3? _LastTargetPoint;
		private bool _DestinationChanged;
		private bool _HavePath;
		private NavMeshPath _Path = new();
		
		private int _TargetPathPointIndex;
		private Vector3 _TargetPathPoint;
		private float _LastPathCalculateTime;

		private float _SqrStopDistance => _MovementData.StopDistance * _MovementData.StopDistance;

		private const float _PathRecalculateTime = 4f;
		
		public override void Init() {
			base.Init();
			_MovementData = Blackboard.Get<BBMovementData>();
			IsUpdated = true;
			IsGizmosUpdated = true;
			_MovementData.DestinationReached = true;
		}
		
		public override TaskStatus Run() {

			TaskStatus status;
			if(_MovementData.TargetPoint == null) {
				_MovementData.MovementType = null;
				return TaskStatus.Failure;
			}

			return TaskStatus.Running;
		}

		protected override void Update() {
			if(!Ship)
				return;
			if(!MovementController)
				return;
			if(!_MovementData.TargetPoint.HasValue)
				return;
			MoveAlongPath();
		}

		private void MoveAlongPath() {
			if (_Path == null) {
				_MovementData.DestinationReached = true;
				return;
			}
			// if (Unit.IsCasting)
			// 	return;
			
			_DestinationChanged = _LastTargetPoint != _MovementData.TargetPoint;
			if (_LastTargetPoint.HasValue) {
				var delta = _LastTargetPoint.Value - _MovementData.TargetPoint.Value;
				if (Vector3.SqrMagnitude(delta) < 0.01)
					_DestinationChanged = false;
			}

			var needRecalculatePath = !_MovementData.DestinationReached && Time.unscaledTime - _LastPathCalculateTime >= _PathRecalculateTime;
			
			if (_DestinationChanged || _HavePath == false || needRecalculatePath) {
				{
					var targetSamplePointFound= NavMesh.SamplePosition(_MovementData.TargetPoint.Value, out var targetPointHit, 10, NavMesh.AllAreas);
					// Debug.LogError("Sample pos calculate");
					if (targetSamplePointFound) {
						_HavePath = NavMesh.CalculatePath(Ship.Position, targetPointHit.position,
							NavMesh.AllAreas, _Path);
						if (!_HavePath) {
							var unitSamplePointFound = NavMesh.SamplePosition(Ship.Position, out var unitPointHit, 10, NavMesh.AllAreas);
							if(unitSamplePointFound)
								_HavePath = NavMesh.CalculatePath(unitPointHit.position, targetPointHit.position,
									NavMesh.AllAreas, _Path);
						}
					
						_LastPathCalculateTime = Time.unscaledTime;
						if (_Path.corners.Length > 1) {
							_TargetPathPointIndex = 1;
							_TargetPathPoint = _Path.corners[_TargetPathPointIndex];
						}
					}
					else {
						_MovementData.TargetPoint = null;
					}
					// Debug.LogError("Calculate path!");
				}
				_LastTargetPoint = _MovementData.TargetPoint;
			}

			var lastPathPoint = _TargetPathPointIndex >= _Path.corners.Length - 1;
			var sqrStopDistance = lastPathPoint ? _SqrStopDistance : 0.25f;
			
			var direction = _TargetPathPoint - Ship.Position;
			var sqrDistToPathPoint = Vector3.SqrMagnitude(direction);
			
			
			if (sqrDistToPathPoint >= sqrStopDistance) {

				direction = direction.normalized;
				
				ApplySeparation(ref direction);
				
				MovementController.Direction = direction.ToVector2XZ();
				MovementController.Gaz = 1f;
				// MovementController.separateFacing = false;
				_MovementData.DestinationReached = false;
			}
			else {
				if(!lastPathPoint) {
					_TargetPathPointIndex++;
					_TargetPathPoint = _Path.corners[_TargetPathPointIndex];
				}
				else {
					MovementController.Direction = Vector2.zero;
					MovementController.Gaz = 0f;
					_MovementData.DestinationReached = true;
				}
			}
		}

		private float SeparationRadius = 5f;
		private float SeparationIntensity = 0.15f;
		private LayerMask SeparationMask = LayerMask.GetMask("Units");
		
		private void ApplySeparation(ref Vector3 direction) {
			var colliders = Physics.OverlapSphere(Ship.Position, SeparationRadius, SeparationMask);
			var shipsAround = new List<Ship>();
			foreach (var col in colliders) {
				if (col.TryGetComponent<Ship>(out var unit)) {
					// if(!unitsAround.Contains(unit))
						shipsAround.Add(unit);
				}
			}
			
			var separationVector = Vector3.zero;
			var separationFactor = 0f;
			foreach (var ship in shipsAround) {
				if(ship == null)
					continue;
				if(Ship == ship)
					continue;
				// if(Team.IsEnemyWith(Unit.Owner,unit.Owner))
				// 	continue;
				var delta = ship.transform.position - Ship.transform.position;
				var distance = delta.magnitude;
				if (distance < SeparationRadius) {
					separationVector -= delta.normalized * (1 - distance / SeparationRadius);
				}
			}
			separationFactor = Mathf.Clamp01(separationVector.magnitude);
			separationVector = separationVector.normalized;

			direction = Vector3.Slerp(direction, separationVector, separationFactor * SeparationIntensity);
			Debug.DrawLine(Ship.Position + Vector3.up, Ship.Position + Vector3.up + separationVector * separationFactor * 5, Color.cyan);
		}

		public override void Dispose() {
			base.Dispose();
			Ship.MovementController.Gaz = 0;
		}

		protected override void OnDrawGizmos() {
#if UNITY_EDITOR
			base.OnDrawGizmos();
			Gizmos.color = Color.white;
			if (_Path.corners.Length > 1) {
				Gizmos.DrawLine(Ship.Position, _Path.corners[_TargetPathPointIndex]);
				for (int i = _TargetPathPointIndex + 1; i < _Path.corners.Length; i++) {
					Gizmos.DrawLine(_Path.corners[i - 1], _Path.corners[i]);
				}
				if(_MovementData.TargetPoint.HasValue)
					Gizmos.DrawWireSphere(_MovementData.TargetPoint.Value, 1f);
			}
			Handles.Label(Ship.Position, _MovementData.MovementType.ToString());
#endif
		}
	}
}