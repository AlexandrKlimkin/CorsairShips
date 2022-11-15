using PestelLib.UI;
using System.Collections.Generic;
using PestelLib.Chests;
using PestelLib.SharedLogic.Modules;
using UnityDI;
using UnityEngine;

namespace Submarines
{
    public class BoxesPackResultScreen : MonoBehaviour
    {
        [SerializeField] protected Transform _container;
        [SerializeField] protected string _drawStyle = "roulette_pack_item";


        public virtual void Initialize(List<ChestsRewardDef> rewards)
        {
            var visualizer = ContainerHolder.Container.Resolve<ChestsRewardVisualizer>();

            foreach (var item in rewards)
            {
                RectTransform rewardItem = visualizer.GetRewardView(item, _drawStyle);
                rewardItem.transform.SetParent(_container);
                rewardItem.transform.position = Vector3.zero;
                rewardItem.transform.rotation = Quaternion.identity;
                rewardItem.transform.localScale = Vector3.one;
            }
        }

        public virtual void Close()
        {
            var gui = ContainerHolder.Container.Resolve<Gui>();
            gui.Close(gameObject);
        }
    }
}