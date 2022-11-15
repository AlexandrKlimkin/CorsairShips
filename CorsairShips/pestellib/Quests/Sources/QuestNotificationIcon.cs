using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityDI;
using PestelLib.SharedLogic.Modules;
using PestelLib.ServerClientUtils;

namespace PestelLib.Quests
{ 
    public class QuestNotificationIcon : MonoBehaviour {
        [Dependency] private QuestModule _questModule;
        [Dependency] private SharedTime _sharedTime;
        [SerializeField] private Image _questDoneImage;
        [SerializeField] private Image _questActiveImage;
        private Sequence _seq;

        void Start () {
            ContainerHolder.Container.BuildUp(this);

            _questModule.OnQuestCompleted.Subscribe(UpdateIcon);
            _questModule.OnListChanged.Subscribe(UpdateIcon);
            _questModule.OnNewQuest.Subscribe(UpdateIcon);

            UpdateIcon();
        }

        void OnDestroy()
        {
            if (_questModule != null)
            {
                _questModule.OnQuestCompleted.Unsubscribe(UpdateIcon);
                _questModule.OnListChanged.Unsubscribe(UpdateIcon);
                _questModule.OnNewQuest.Unsubscribe(UpdateIcon);
            }
            TryToKillSeq();
        }

        void OnEnable()
        {
            if (_questModule != null)
            {
                UpdateIcon();
            }
        }

        private void TryToKillSeq()
        {
            if (_seq != null)
            {
                _seq.Kill();
                _seq = null;
            }
        }

        private void UpdateIcon(string questId)
        {
            UpdateIcon();
        }

        private void UpdateIcon(QuestDef questDef)
        {
            UpdateIcon();
        }

        [ContextMenu("Update")]
        public void UpdateIcon () {
            _questDoneImage.enabled = _questModule.AnyCompletedQuests;
            _questActiveImage.enabled = !_questModule.AnyCompletedQuests && _questModule.HasActiveQuests(_sharedTime.Now);

            if (_questDoneImage.enabled)
            {
                TryToKillSeq();

                _seq = DOTween.Sequence();
                _seq.Append(
                    DOTween.To(
                    () => _questDoneImage.color.a,
                    x => _questDoneImage.color = new Color(_questDoneImage.color.r, _questDoneImage.color.g, _questDoneImage.color.b, x),
                    1f, 0.55f
                    )
                );
                _seq.Append(
                    DOTween.To(
                    () => _questDoneImage.color.a,
                    x => _questDoneImage.color = new Color(_questDoneImage.color.r, _questDoneImage.color.g, _questDoneImage.color.b, x),
                    0.2f, 1.25f
                    )
                );
                _seq.SetLoops(-1);
                _seq.Play();
            }
        }
    }
}