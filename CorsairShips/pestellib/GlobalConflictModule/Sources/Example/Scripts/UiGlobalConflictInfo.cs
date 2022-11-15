using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using ServerShared.GlobalConflict;
using UnityEngine;
using UnityEngine.UI;
using UnityDI;
using ServerShared.Sources.GlobalConflict;
using S;
using UnityEngine.Experimental.UIElements;
using Button = UnityEngine.UI.Button;

namespace GlobalConflict.Example
{
    public class UiGlobalConflictInfo : MonoBehaviour
    {
        [SerializeField]
        private Text _txtInConflict;
        [SerializeField]
        private Text _txtStage;
        [SerializeField]
        private Text _txtStageEnds;
        [SerializeField]
        private Button _btnAutoJoin;
        [SerializeField]
        private Dropdown _cbJoinTeamSelector;
        [SerializeField]
        private Button _btnJoinManual;
        [SerializeField]
        private Text _txtConflictId;
        [SerializeField] private Text _txtImInConflict;
        [SerializeField] private Text _txtPlayerId;
        [SerializeField] private Text _txtPlayerTeamId;
        [SerializeField]
        private Text _txtWinPoints;
        [SerializeField]
        private Text _txtDonations;
        [SerializeField]
        private Text _txtGeneralLevel;
        [SerializeField]
        private Text _txtRegTime;
        [SerializeField]
        private Text _txtPlayerDonationBonuses;
        [SerializeField]
        private Text _txtEnergy;
        [SerializeField]
        private Text _txtTeamWinPoints;
        [SerializeField]
        private Text _txtTeamDonations;
        [SerializeField]
        private Text _txtTeamResultPoints;
        [SerializeField]
        private Text _txtTeamDonationBonuses;

        [SerializeField] private Button _btnTeamDontatorsTop;
        [SerializeField]
        private Button _btnTeamWinPointsTop;
        [SerializeField]
        private Button _btnStartConflict;

        [SerializeField] private InputField _tbAddTimeAmount;
        [SerializeField] private Button _btnAddTime;

        [SerializeField] private Button _btnRefresh;

        [SerializeField] private InputField _tbDonation;
        [SerializeField] private Button _btnDonate;
        [SerializeField] private Dropdown _ddProto;
        [SerializeField] private Button _btnDeployQuest;
        [Dependency]
        private GlobalConflictModuleBase _globalConflict;

        private string _conflictId;
        private bool _inited;

        private ClassWithHacks.PrototypeSource[] _protoSources;


        IEnumerator Start()
        {
            while (!GlobConflictInitializer.Initialized)
            {
                yield return new WaitForSeconds(0.5f);
            }
            ContainerHolder.Container.BuildUp(this);

            _btnAutoJoin.onClick.AddListener(OnAutoJoinClick);
            _btnJoinManual.onClick.AddListener(OnManualJoinClick);
            _btnDonate.onClick.AddListener(OnDonateClick);
            _btnRefresh.onClick.AddListener(OnRefresh);
            _btnTeamDontatorsTop.onClick.AddListener(() => OnDumpDonatorsTop(true));
            _btnTeamWinPointsTop.onClick.AddListener(() => OnDumpWinPointsTop(true));
            _btnStartConflict.onClick.AddListener(OnStartConflict);
            _btnAddTime.onClick.AddListener(OnAddTime);
            _btnDeployQuest.onClick.AddListener(OnDeployQuest);

            _globalConflict.Initalized += OnInitDone;
            _globalConflict.PlayerStateUpdated += OnPlayerUpdated;
        }

        private void OnDeployQuest()
        {
            var cmd = new GlobalConflictModuleBase_DeployQuest();
            var result = CommandProcessor.Process<bool, GlobalConflictModuleBase_DeployQuest>(cmd);
            Debug.Log("DeployQuest result: " + result);
            if (result)
                _globalConflict.Init();
        }

        // DONT USE IN PRODUCTION ENVIRONMENT
        private void OnAddTime()
        {
            var timeToAdd = TimeSpan.Zero;
            if (!TimeSpan.TryParse(_tbAddTimeAmount.text, out timeToAdd))
            {
                Debug.LogError("Cant parse TimeStamp. Use format <days>.<hours>:<minutes>:<seconds>.");
                return;
            }

            if (timeToAdd <= TimeSpan.Zero)
            {
                Debug.LogError("Add time argument is invalid " + timeToAdd);
                return;
            }

            ClassWithHacks.AddTime(timeToAdd, () => _globalConflict.Init());
        }

        // DONT USE IN PRODUCTION ENVIRONMENT
        private void OnStartConflict()
        {
            var idx = _ddProto.value;
            var source = _protoSources[idx];
            ClassWithHacks.StartNow(source, OnConflictStarted);
        }

        private void OnConflictStarted(string s)
        {
            Debug.Log("Conflict '" + s + "' just started");
            _globalConflict.Init();
        }

        private void OnDumpWinPointsTop(bool myTeamOnly)
        {
            _globalConflict.GetTopWinPoints(myTeamOnly, (top, myPlace) =>
            {
                Debug.Log(_globalConflict.ConflictState.Id + ": Top win points");
                var place = 0;
                foreach (var playerState in top)
                {
                    Debug.Log(++place + ": " + JsonConvert.SerializeObject(playerState));
                }
                Debug.Log("My place is " + myPlace);
            });
        }

        private void OnDumpDonatorsTop(bool myTeamOnly)
        {
            _globalConflict.GetTopDonators(myTeamOnly, (top, myPlace) =>
            {
                Debug.Log(_globalConflict.ConflictState.Id + ": Top donators");
                var place = 0;
                foreach (var playerState in top)
                {
                    Debug.Log(++place + ": " + JsonConvert.SerializeObject(playerState));
                }
                Debug.Log("My place is " + myPlace);
            });
        }

        private void OnRefresh()
        {
            _globalConflict.Init(() =>
            {
                var cmd = new GlobalConflictModuleBase_RecalcEnergy();
                CommandProcessor.Process<object, GlobalConflictModuleBase_RecalcEnergy>(cmd);
            });
        }

        private void OnPlayerUpdated()
        {
            Debug.Log("Player updated");
        }

        private void OnDonateClick()
        { 
            _globalConflict.AddDonation(int.Parse(_tbDonation.text), DonationComplete);
        }

        private void DonationComplete()
        {
            Debug.Log("Donation complete");
            ReloadConflict();
        }

        private void OnManualJoinClick()
        {
            _globalConflict.Register(_cbJoinTeamSelector.itemText.text, OnJoinComplete);
        }

        private void OnAutoJoinClick()
        {
            _globalConflict.Register(OnJoinComplete);
        }

        private void OnJoinComplete()
        {
            Debug.Log("Join complete");
        }

        private void ReloadConflict()
        {
            _globalConflict.Init();
        }

        private void OnInitDone()
        {
            Debug.Log("Init done");

            
        }

        void CleanOrNot()
        {
            string defStr = "N/A";
            if (_globalConflict.ConflictState == null)
            {
                if (_conflictId == "none")
                    return;
            }
            else if(_conflictId == _globalConflict.ConflictState.Id)
                return;
            _txtEnergy.text = _txtPlayerId.text = _txtConflictId.text = _txtStageEnds.text = _txtStage.text = _txtImInConflict.text = _txtInConflict.text = defStr;
            _txtTeamWinPoints.text = _txtTeamResultPoints.text = _txtTeamDonationBonuses.text = _txtTeamDonations.text = defStr;
            _conflictId = _globalConflict.ConflictState != null ? _globalConflict.ConflictState.Id : "none";
            _cbJoinTeamSelector.options.Clear();
            _tbDonation.interactable = _btnDonate.interactable = _btnAutoJoin.interactable = _btnJoinManual.interactable =_cbJoinTeamSelector.interactable = false;
        }

        void Update()
        {
            if(_globalConflict == null)
                return;

            CleanOrNot();

            if (_protoSources == null)
            {
                ClassWithHacks.GetSources(sources =>
                {
                    _protoSources = sources;
                    _ddProto.AddOptions(sources.Select(_ => _.Description).ToList());
                });
                _protoSources = new ClassWithHacks.PrototypeSource[] {};
            }

            if (!_inited && _globalConflict.ConflictState != null)
            {
                OnInitDone();
                _inited = true;
            }

            _txtInConflict.text = _globalConflict.HasActiveConflict.ToString();
            _txtImInConflict.text = _globalConflict.IamInConflict.ToString();
            if (_globalConflict.HasActiveConflict)
            {
                _txtConflictId.text = _conflictId;
            }

            _txtRegTime.text = _txtGeneralLevel.text = _txtDonations.text = _txtWinPoints.text = _txtPlayerTeamId.text = _txtPlayerId.text = "N/A";
            if (_globalConflict.ConflictState == null)
            {
                _btnAutoJoin.interactable = _cbJoinTeamSelector.interactable = _btnJoinManual.interactable = false;
            }
            else if (!_globalConflict.IamInConflict)
            {
                var auto = _globalConflict.ConflictState.AssignType == TeamAssignType.BasicAuto;
                _btnAutoJoin.interactable = auto;
                _cbJoinTeamSelector.interactable = !auto;
                _btnJoinManual.interactable = !auto;
            }
            else
            {
                _btnAutoJoin.interactable = _cbJoinTeamSelector.interactable = _btnJoinManual.interactable = false;
                var playerState = _globalConflict.PlayerState;
                _txtPlayerId.text = playerState.Id;
                _txtPlayerTeamId.text = playerState.TeamId;
                _txtWinPoints.text = playerState.WinPoints.ToString();
                _txtDonations.text = playerState.DonationPoints.ToString();
                _txtGeneralLevel.text = playerState.GeneralLevel.ToString();
                _txtRegTime.text = playerState.RegisterTime.ToString();
                _txtPlayerDonationBonuses.text = string.Join(",", playerState.DonationBonuses.Select(_ => _.ClientType + "=" + _.Value).ToArray());
                _txtEnergy.text = _globalConflict.CurrentEnergy + "/" + _globalConflict.MaxEnergy;
                var teamState = _globalConflict.ConflictState.TeamsStates.First(_ => _.Id == playerState.TeamId);
                _txtTeamWinPoints.text = teamState.WinPoints.ToString();
                _txtTeamDonations.text = teamState.DonationPoints.ToString();
                _txtTeamDonationBonuses.text = string.Join(",", teamState.DonationBonuses.Select(_ => _.ClientType + "=" + _.Value).ToArray());
                _txtTeamResultPoints.text = teamState.ResultPoints.ToString();
            }

            if (_cbJoinTeamSelector.enabled && _globalConflict.ConflictState != null)
            {
                if (_cbJoinTeamSelector.options.Count == 0)
                {
                    _cbJoinTeamSelector.AddOptions(_globalConflict.ConflictState.Teams.ToList());
                }
            }

            if (_globalConflict.HasActiveConflict)
            {
                var stage = _globalConflict.GetCurrentStage();
                _txtStage.text = stage.Id.ToString();
                _txtStageEnds.text = (stage.End - DateTime.UtcNow).ToString();

                var inDonationStage = stage.Id == StageType.Donation && _globalConflict.IamInConflict;
                _tbDonation.interactable = _btnDonate.interactable = inDonationStage;
            }

            _btnTeamDontatorsTop.interactable = _globalConflict.HasActiveConflict;
            _btnTeamWinPointsTop.interactable = _globalConflict.HasActiveConflict;
            _tbAddTimeAmount.interactable = _globalConflict.HasActiveConflict;
            _btnAddTime.interactable = _globalConflict.HasActiveConflict;
            _btnDeployQuest.interactable = _globalConflict.CanDeployQuests;
        }
    }
}
