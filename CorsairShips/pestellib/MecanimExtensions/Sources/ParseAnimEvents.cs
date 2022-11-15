using System.Linq;
using PestelLib.MecanimExtensions;
using UnityEngine;

public class ParseAnimEvents : StateMachineBehaviour
{
    private AnimEventProcessor _animEventProcessor;
    private AnimationClip _mainClip;
    private float _enterToStateTimestamp;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_animEventProcessor == null)
        {
            _animEventProcessor = animator.GetComponent<AnimEventProcessor>();
        }

        var actualClips = animator.GetNextAnimatorClipInfo(layerIndex);

        foreach (var clip in actualClips)
        {
            foreach (var evt in clip.clip.events.OrderBy(e => e.time))
            {
                _animEventProcessor.AddEvent(evt, clip.clip);
            }
        }
    }
}
