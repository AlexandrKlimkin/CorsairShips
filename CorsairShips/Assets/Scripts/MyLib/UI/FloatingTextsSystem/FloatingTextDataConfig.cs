using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.FloatingTexts {
	[CreateAssetMenu(fileName = "FloatingTextDataConfig", menuName = "Data/FloatingTextDataConfig")]
	public class FloatingTextDataConfig : SingletonScriptableObject<FloatingTextDataConfig> {
		[Header("Templates")]
		public FloatingTextData NotificationUsual;
		public FloatingTextData FloatingDmg;
		public FloatingTextData FloatingHeal;

		[Header("Queues")]
		[SerializeField]
		private List<FloatingTextQueueData> _QueueDatas;

		private Dictionary<QueueType, FloatingTextQueueData> _QueueDict;
		public IReadOnlyDictionary<QueueType, FloatingTextQueueData> QueueDict => _QueueDict;

		private void OnEnable() {
			_QueueDict = _QueueDatas.ToDictionary(_=>_.QueueType);
		}
	}
}