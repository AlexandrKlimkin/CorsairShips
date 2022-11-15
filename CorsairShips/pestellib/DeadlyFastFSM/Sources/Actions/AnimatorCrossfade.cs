using System;
using PestelLib.MecanimTransitionStorage;
using UnityEngine;

namespace PestelLib.DeadlyFastFSM.Actions
{
    public class AnimatorCrossfade : IStateAction
    {
        public Action OnFinish { get; set; }


        public string StateName
        {
            get { return _stateName; }
            set
            {
                _stateName = value;
                _anims = StateName.Split('|');
            }
        }

        public float TransitTime = 0.25f;
        public int LayerIndex;
        public bool WaitUntilFinish;
        public string SelectedStateName;
        public bool PlayAnimsInRow;
        public bool AlwaysInstantTransition;

        private String _stateName;
        private AnimatorStateInfo _stateInfo;
        private bool _played;
        private Animator _animator;
        private static TransitionStorage _transitions;
        private float _startTime;
        private string[] _anims;
        private int _playNumber;
        private MonoBehaviour _owner;

        private Animator Animator
        {
            //get { return _animator ?? (_animator = Owner.GetComponentInChildren<Animator>()); }
            get
            {
                return _animator = _owner.GetComponentInChildren<Animator>(); //TODO: FIX IT
            }
        }

        public AnimatorCrossfade(MonoBehaviour owner)
        {
            _owner = owner;
            OnFinish = () => { };

            if (_transitions == null)
            {
                _transitions = Resources.Load<TransitionStorage>("transitions");
            }
        }

        public void OnEnter()
        {
            if (Animator == null) return;

            _startTime = Time.time;
            _played = false;
            var current = Animator.GetCurrentAnimatorStateInfo(LayerIndex);

            if (_anims.Length > 1)
            {
                if (PlayAnimsInRow)
                {
                    var index = _playNumber++ % _anims.Length;
                    SelectedStateName = _anims[index];
                }
                else
                {
                    SelectedStateName = _anims[UnityEngine.Random.Range(0, _anims.Length)];
                }
            }
            else
            {
                SelectedStateName = StateName;
            }

            var actualTransitionTime = TransitTime / current.length;

            if (AlwaysInstantTransition)
            {
                actualTransitionTime = 0;
            }
            else if (_transitions.TransitionsDictionary.ContainsKey(current.shortNameHash))
            {
                if (_transitions.TransitionsDictionary[current.shortNameHash].ContainsKey(SelectedStateName))
                {
                    actualTransitionTime = _transitions.TransitionsDictionary[current.shortNameHash][SelectedStateName];
                    //Debug.Log("Override transition to " + actualTransitionTime);
                }
            }

            Animator.CrossFade(SelectedStateName, actualTransitionTime, LayerIndex, 0f);
            //Debug.Log(Time.frameCount + " crossfade " + stateName);
        }

        public void Update()
        {
            _stateInfo = Animator.GetCurrentAnimatorStateInfo(LayerIndex);

            if (WaitUntilFinish)
            {

                if (_stateInfo.IsName(SelectedStateName))
                {

                    _played = true;
                    if (ElapsedTime >= (_stateInfo.length / Animator.speed))
                        EndAction();

                }
                else if (_played)
                {

                    EndAction();
                }

            }
            else
            {

                if (ElapsedTime >= TransitTime)
                    EndAction();
            }
        }

        private void EndAction()
        {
            OnFinish();
        }

        public float ElapsedTime
        {
            get
            {
                return Time.time - _startTime;
            }
        }
    }
}
