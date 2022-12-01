using PestelLib.Utils;
using Tools.BehaviourTree;
using UnityDI;

namespace AI {
	public abstract class UpdatedTask : Task {
		[Dependency]
		protected readonly UnityEventsProvider _UnityEventProvider;

		protected Blackboard Blackboard => BehaviourTree.Blackboard;

		protected bool _IsUpdated = false;
		public bool IsUpdated {
			get => _IsUpdated;
			set
			{
				if (_IsUpdated == value) return;
				_IsUpdated = value;
				if(value)
					_UnityEventProvider.OnUpdate += UpdateRunningTask;
				else
					_UnityEventProvider.OnUpdate -= UpdateRunningTask;
			}
		}

		protected bool _IsGizmosUpdated;
		public bool IsGizmosUpdated {
			get => _IsGizmosUpdated;
			set
			{
				if (_IsGizmosUpdated == value) return;
				_IsGizmosUpdated = value;
				if(value)
					_UnityEventProvider.OnGizmos += OnDrawGizmos;
				else
					_UnityEventProvider.OnGizmos -= OnDrawGizmos;
			}
		}

		public override void Init() {
			ContainerHolder.Container.BuildUp(GetType(), this);
		}

		private void UpdateRunningTask() {
			if(Status == TaskStatus.Running)
				Update();
		}

		protected virtual void Update() { }

		protected virtual void OnDrawGizmos() { }

		public override void Begin() { }

		public override void Dispose() {
			base.Dispose();
			IsUpdated = false;
			IsGizmosUpdated = false;
		}
	}
}