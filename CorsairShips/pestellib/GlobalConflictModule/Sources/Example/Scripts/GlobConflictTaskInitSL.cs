using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using GlobalConflictModule.Example;
using GlobalConflictModule.Sources.Scripts;
using MessagePack;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicBase;
using PestelLib.TaskQueueLib;
using PestelLib.Utils;
using S;
using ServerShared.GlobalConflict;
using UnityDI;
using UnityEngine;

namespace GlobalConflict.Example
{
    public class GlobConflictTaskInitSL : Task
    {
        //TODO: change user feature
        private static Guid PlayerId = new Guid("6e15b058-1c8f-4525-934c-9fbc6f89d691");
        [Dependency] private GlobConflictTestDefinitions _definitions;
        [Dependency] private RequestQueue _requestQueue;
        [Dependency] private UpdateProvider _updateProvider;

        public override void Run()
        {
            ContainerHolder.Container.BuildUp(this);
            _updateProvider.OnUpdate += OnUpdate;
        }

        private void OnUpdate()
        {
            if(string.IsNullOrEmpty(UiSelectPlayer.PlayerId))
                return;

            _updateProvider.OnUpdate -= OnUpdate;
            _requestQueue.PlayerId = new Guid(UiSelectPlayer.PlayerId);
            _requestQueue.SendRequest(
                "InitRequest",
                new Request
                {
                    InitRequest = new InitRequest { }
                }, (response, collection) =>
                {
                    InitDone(collection.Data);
                }
            );
        }

        private void InitDone(byte[] state)
        {
            var userProfile = MessagePackSerializer.Deserialize<UserProfile>(state);
            var sharedLogic = new SharedLogicDefault<GlobConflictTestDefinitions>(userProfile, _definitions);

            ContainerHolder.Container.RegisterInstance<ISharedLogic>(sharedLogic);
            ContainerHolder.Container.RegisterCustom(() => sharedLogic.GetModule<GlobalConflictModuleBase>());

            sharedLogic.OnLogMessage += delegate (string s) { Debug.Log("SL: " + s); };

            OnComplete(this);
        }
    }
}
