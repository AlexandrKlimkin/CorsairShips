using PestelLib.SharedLogic.Modules;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.Chests
{
    public class ChestScreenItem : MonoBehaviour
    {
        [SerializeField] private Text _caption;
        [SerializeField] private Text _description;
        [SerializeField] private Text _itemLevel;
        [SerializeField] private Image _image;
        [SerializeField] private Image _background;
        

        private ChestsRewardVisualData _chestsRewardVisualData;

        public ChestsRewardVisualData ChestsRewardVisualData {
            get { return _chestsRewardVisualData; }
            set
            {
                _chestsRewardVisualData = value;
                _caption.text = value.Name;
                _description.text = value.Description;
                _image.sprite = value.Icon;
                _image.color = value.IconColor;

                if (_background != null)
                    _background.color = _chestsRewardVisualData.BackgroundColor;

                if (_itemLevel != null && !string.IsNullOrEmpty(_chestsRewardVisualData.RewardLevelString))
                    _itemLevel.text = _chestsRewardVisualData.RewardLevelString;

                _image.transform.localScale = ChestsRewardVisualData.IconScale;
            }
        }
    }
}