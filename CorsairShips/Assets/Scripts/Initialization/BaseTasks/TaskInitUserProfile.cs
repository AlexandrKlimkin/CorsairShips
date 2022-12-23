using System;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic;
using PestelLib.SharedLogicBase;
using PestelLib.TaskQueueLib;
using S;
using PestelLib.SaveSystem;
using PestelLib.UniversalSerializer;
using UnityDI;
using UnityEngine;
using PestelLib.SharedLogicClient;

namespace Game.Initialization.Base {
    public class TaskInitUserProfile : Task {
        [Dependency]
        private IStorage _storage;

        [Dependency]
        private Definitions _defs;

        [Dependency]
        private RequestQueue _requestQueue;

        private SharedLogicCore _sharedLogic;

        override public void Run() {
            ContainerHolder.Container.BuildUp(this);

            //#if DEVELOPMENT_BUILD || UNITY_EDITOR
            //if (_config.UseLocalState)
            //{
            UserProfile state;
            if (!_storage.IsStorageEmpty)
                state = LoadLocalState();
            else {
                var factory = new DefaultStateFactory();
                state = factory.MakeDefaultState(Guid.Empty);
            }

            MakeSharedLogic(state);
            OnComplete(this);
            //}
            //else
            //    #endif
            //    SendInitRequest();
        }

        //#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private UserProfile LoadLocalState() {
            if (!_storage.IsStorageEmpty) {
                return Serializer.Deserialize<UserProfile>(_storage.UserProfile);
            }
            else {
                var stateFactory = new DefaultStateFactory();
                return stateFactory.MakeDefaultState(Guid.NewGuid());
            }
        }
        //#endif

        private void MakeSharedLogic(UserProfile state) {
            bool defaultState = state.ModulesDict == null || state.ModulesDict.Count == 0;
            _sharedLogic = new SharedLogicCore(state, _defs);
            _sharedLogic.OnLogMessage += msg => Debug.Log($"SL: {msg}");
            ContainerHolder.Container.RegisterInstance(_sharedLogic);
            ContainerHolder.Container.RegisterInstance<ISharedLogic>(_sharedLogic);

            _sharedLogic.RegisterModulesInContainer(ContainerHolder.Container);

            //SharedLogicCommand.InitModule.ClientLoadingFinished();

            SharedLogicSettings.IsDebug = Application.isEditor;

            /*
            if (defaultState) {
                var _desertWarsPurchaser = ContainerHolder.Container.Resolve<DesertWarsPurchaser>();
                _desertWarsPurchaser.RestorePurchases();
            }*/

            // ??
            //UnityEngine.CrashReportHandler.CrashReportHandler.SetUserMetadata("SL PlayerId", _requestQueue.PlayerId.ToString());
        }
    }
}
