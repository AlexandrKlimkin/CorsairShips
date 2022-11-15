using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.Quests
{ 
	public class QuestNotificationScreen : MonoBehaviour {
        [SerializeField] private Text _text;
	    [SerializeField] private RectTransform _animRoot;
	    [SerializeField] private Vector2 _animatedOffset;
	    private Sequence _seq;
	    private Queue<string> _messagesToShow = new Queue<string>();

	    public void ShowMessage(string message)
	    {
	        _messagesToShow.Enqueue(message);
            UpdateQueue();
	    }

	    void UpdateQueue()
	    {
	        if (_messagesToShow.Count == 0 || _seq != null)
	        {
	            return;
	        }
            
            var message = _messagesToShow.Dequeue();

	        _text.text = message;

	        _seq = DOTween.Sequence();
	        _animRoot.anchoredPosition = _animatedOffset;
	        _seq.Append(DOTween.To(() => _animRoot.anchoredPosition, x => _animRoot.anchoredPosition = x, Vector2.zero,
	            0.5f));
	        _seq.Append(
	            DOTween.To(() => _animRoot.anchoredPosition, x => _animRoot.anchoredPosition = x, _animatedOffset, 0.5f)
	                .SetDelay(4f));
	        _seq.Play();

	        _seq.OnComplete(() =>
	        {
	            _seq = null;
                UpdateQueue();
	        });
        }

	    private void OnDestroy()
	    {
	        KillTween();
	    }

	    private void KillTween()
	    {
	        if (_seq != null)
	        {
	            _seq.Kill();
	            _seq = null;
	        }
	    }
	}
}