using System.Collections.Generic;
using System.Linq;
using PestelLib.SharedLogic.Modules;
using UnityDI;
using UnityEngine;

namespace PestelLib.Chests
{
    public class ChestsRewardVisualizer : MonoBehaviour
    {
        [SerializeField] private List<ChestsRewardDrawer> _presets;

        private void Start()
        {
            foreach (var preset in _presets)
            {
                preset.gameObject.SetActive(false);
            }
        }

        public RectTransform GetRewardView(ChestsRewardDef chestsRewardDef, string style)
        {
            var preset = _presets.FirstOrDefault(x => x.name == style);
            if (preset == null)
            {
                preset = _presets[0];
            }

            return preset.Setup(chestsRewardDef);
        }

        public List<RectTransform> GetRewardView(List<ChestsRewardDef> chestsRewardDef, string style)
        {
            var result = new List<RectTransform>();
            var preset = _presets.FirstOrDefault(x => x.name == style);
            if (preset == null)
            {
                preset = _presets[0];
            }

            foreach (var rewardDef in chestsRewardDef)
            {
                result.Add(preset.Setup(rewardDef));
            }

            return result;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F11) && Input.GetKey(KeyCode.RightControl))
            {
                var defs = ContainerHolder.Container.Resolve<List<ChestsRewardDef>>();
                GetRewardView(defs, "default");
            }
#endif
        }
        [ContextMenu("UpdateList")]
        private void UpdateList()
        {
            _presets.Clear();
            for (int i = 0; i < this.transform.childCount; i++)
            {
                ChestsRewardDrawer drawer = transform.GetChild(i).GetComponent<ChestsRewardDrawer>();
                if (drawer)
                {
                    _presets.Add(drawer);
                }
            }
        }
    }
}