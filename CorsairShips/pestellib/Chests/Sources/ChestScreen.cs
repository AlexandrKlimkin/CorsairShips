using PestelLib.SharedLogic.Modules;
using PestelLib.UI;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.Chests
{
    public class ChestScreen : MonoBehaviour
    {
        [Dependency] private IChestsConcreteGameInterface _chestsConcreteGameInterface;
        [Dependency] private ChestModule _chestModule;
        [Dependency] private Gui _gui;

        [SerializeField] private ChestManager _chestManagerPrefab;

        private void Start()
        {
            ContainerHolder.Container.BuildUp(this);
        }


        #region Debug

        [SerializeField] private InputField _debugInputField;
        private ChestManager _chestManager;

        public void DebugGiveChestByRarity()
        {
            //OpenChest(int.Parse(_debugInputField.text));

            var rarity = int.Parse(_debugInputField.text);
            
            _chestManager = Instantiate(_chestManagerPrefab);
            _chestManager.ChestRarity = rarity;
            _chestManager.ChestNumber = rarity;
            _chestManager.OnClose += OnClose;

            gameObject.SetActive(false);
        }

        private void OnClose()
        {
            _chestManager.OnClose -= OnClose;

            gameObject.SetActive(true);
        }
        #endregion
    }
}