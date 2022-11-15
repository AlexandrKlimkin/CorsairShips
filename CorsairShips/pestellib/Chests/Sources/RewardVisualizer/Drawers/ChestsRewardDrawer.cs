using PestelLib.SharedLogic.Modules;
using UnityEngine;

namespace PestelLib.Chests
{
    public abstract class ChestsRewardDrawer : MonoBehaviour
    {
        public virtual RectTransform Setup(ChestsRewardDef rewardDef)
        {
            var instanceGo = Instantiate(gameObject);
            var instance = instanceGo.GetComponent<ChestsRewardDrawer>();

            instance.Draw(rewardDef);
            instance.gameObject.SetActive(true);

            return instance.GetComponent<RectTransform>();
        }

        public abstract void Draw(ChestsRewardDef rewardDef);
    }
}
