using Game;
using PestelLib.Localization;
using System;
using System.Collections.Generic;
using PestelLib.SharedLogic.Extentions;
using UnityDI;
using UnityEngine;

namespace UI.FloatingTexts {
    public class FloatingTextsSystem : MonoBehaviour {
        [Dependency]
        private readonly ILocalization _Localization;

        [SerializeField]
        private FloatingTextWidget _FloatingTextWidgetPrefab;

        [SerializeField]
        private RectTransform _WidgetsRoot;

        [SerializeField]
        private Transform _NotificationsFromPoint;

        [SerializeField]
        private Transform _NotificationsToPoint;

        private MonoBehaviourPool<FloatingTextWidget> _WidgetsPool;

        private Dictionary<QueueType, Queue<FloatingTextData>> _TextsQueuesDict =
            new Dictionary<QueueType, Queue<FloatingTextData>>();

        private Dictionary<QueueType, float> _QueueTimers = new Dictionary<QueueType, float>();

        public Transform NotificationsFromPoint => _NotificationsFromPoint;
        public Transform NotificationsToPoint => _NotificationsToPoint;
        
        private Camera Camera {
            get {
                if (!_Camera)
                    _Camera = Camera.main;
                return _Camera;
            }
        }

        private Camera _Camera;

        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
            ContainerHolder.Container.RegisterInstance(this);
            _WidgetsPool = new MonoBehaviourPool<FloatingTextWidget>(_FloatingTextWidgetPrefab, _WidgetsRoot);

            var values = Enum.GetValues(typeof(QueueType));
            foreach (var val in values) {
                var enumVal = (QueueType)val;
                if (enumVal == QueueType.None)
                    continue;
                _TextsQueuesDict.Add(enumVal, new Queue<FloatingTextData>());
                _QueueTimers.Add(enumVal, float.NegativeInfinity);
            }
        }

        private void Update() {
            // if (Input.GetKeyDown(KeyCode.Q)) {
            //     PlayNotification(_Localization.Get("notification/nightIsComing"));
            // }

            var time = Time.time;

            foreach (var pair in _TextsQueuesDict) {
                if (pair.Value.Count == 0)
                    continue;
                var delay = FloatingTextDataConfig.Instance.QueueDict[pair.Key].Delay;
                if (time - _QueueTimers[pair.Key] < delay)
                    continue;
                var widget = pair.Value.Dequeue();
                PlayWidget(widget);
                _QueueTimers[pair.Key] = Time.time;
            }
        }

        // private void AddToQueue(QueueType queueType, FloatingTextData data) {
        //     _TextsQueuesDict[queueType].Enqueue(data);
        // }
        //
        // private FloatingTextData GetFromQueue(QueueType queueType) {
        //     return _TextsQueuesDict[queueType].Dequeue();
        // }

        public void PlayNotification(string text, QueueType queueType = QueueType.Notifications) {
            var data = FloatingTextDataConfig.Instance.NotificationUsual;
            data.Text = text;
            data.FromPos = _NotificationsFromPoint.localPosition;
            data.ToPos = _NotificationsToPoint.localPosition;
            PlayCanvasPos(data, queueType);
        }

        public void PlayWorldPos(FloatingTextData data, QueueType queueType = QueueType.None) {
            var viewportPos = Camera.WorldToViewportPoint(data.FromPos);

            if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
                return;

            var rect = _WidgetsRoot.rect;
            var screenPosition = new Vector2(
                (viewportPos.x * rect.width),
                (viewportPos.y * rect.height));

            data.FromPos = screenPosition;
            PlayCanvasPos(data, queueType);
        }

        public void PlayCanvasPos(FloatingTextData data, QueueType queueType = QueueType.None) {
            if (queueType == QueueType.None) {
                PlayWidget(data);
            }
            else {
                _TextsQueuesDict[queueType].Enqueue(data);
            }
        }

        private void PlayWidget(FloatingTextData data) {
            var widget = _WidgetsPool.GetObject();
            widget.OnComplete -= OnComplete;
            widget.OnComplete += OnComplete;
            widget.Play(data);
        }

        private void OnComplete(FloatingTextWidget widget) {
            widget.OnComplete -= OnComplete;
            _WidgetsPool.ReturnObjectToPool(widget);
        }

        private void OnDestroy() {
            ContainerHolder.Container.UnregisterInstance(this);
        }
    }
}