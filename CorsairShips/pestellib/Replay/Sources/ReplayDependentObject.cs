using System.Collections.Generic;
using UnityDI;
using UnityEngine;

namespace PestelLib.Replay
{
    public class ReplayDependentObject : InitableMonoBehaviour
    {
        [Dependency] private ReplayManager _replayManager;

        [SerializeField] private List<Behaviour> _disableInReplay;
        
        protected override void SafeStart()
        {
            base.SafeStart();

            _replayManager.OnReplayBegin += ReplayManagerOnOnReplayBegin;
            _replayManager.OnReplayEnd += ReplayManagerOnOnReplayEnd;
        }

        private void OnDestroy()
        {
            if (_replayManager != null)
            {
                _replayManager.OnReplayBegin -= ReplayManagerOnOnReplayBegin;
                _replayManager.OnReplayEnd -= ReplayManagerOnOnReplayEnd;
            }
        }

        private void ReplayManagerOnOnReplayEnd()
        {
            _disableInReplay.ForEach(obj => obj.enabled = true);
        }

        private void ReplayManagerOnOnReplayBegin()
        {
            _disableInReplay.ForEach(obj => obj.enabled = false);
        }
    }
}
