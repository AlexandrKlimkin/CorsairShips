using System;
using System.Collections;
using System.Collections.Generic;
using MessagePack;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic;
using PestelLib.SharedLogicBase;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using S;
using UnityDI;
using UnityEngine;
using PestelLib.Utils;

namespace PestelLib.Leagues
{ 
	public class TaskInitScene : Task
	{
#pragma warning disable 649
		[Dependency]
	    private RequestQueue _requestQueue;
	    [Dependency]
	    private UpdateProvider _updateProvider;
	    [Dependency]
        private LeaguesModuleDefinitionsContainer _leaguesModuleDefinitionsContainer;
#pragma warning restore 649

		public override void Run()
	    {
            ContainerHolder.Container.BuildUp(this);

	        LoadRemoteState();
	    }

	    private void LoadRemoteState()
	    {
	        //_unityEventsProvider.OnApplicationPaused += OnApplicationPaused;
	        SendInitRequest();
	    }

	    private void SendInitRequest()
	    {
	        if (Application.isEditor)
	            Debug.Log("Send init request");

	        _requestQueue.SendRequest(
	            "InitRequest",
	            new Request
	            {
	                InitRequest = new InitRequest()
	            },
	            OnRemoteStateLoaded
	        );
	    }

	    private void OnRemoteStateLoaded(Response r, DataCollection container)
	    {
	        if (Application.isEditor)
	            Debug.Log("OnRemoteStateLoaded");

	        var remoteStateBytes = container.Data;
	        var remoteState = MessagePackSerializer.Deserialize<UserProfile>(remoteStateBytes);

	        MakeSharedLogic(remoteState);
	        _updateProvider.StartCoroutine(SendOnComplete());
        }

	    IEnumerator SendOnComplete()
	    {
	        yield return new WaitForSeconds(0.1f);

	        OnComplete(this);
	    }

        private void MakeSharedLogic(UserProfile state)
	    {
	        var sharedLogic = new SharedLogicDefault<LeaguesModuleTestDefinitions>(state, _leaguesModuleDefinitionsContainer.SharedLogicDefs);
	        sharedLogic.OnLogMessage += s => { Debug.Log("SL: " + s); };
	        ContainerHolder.Container.RegisterInstance(sharedLogic);
	        ContainerHolder.Container.RegisterInstance<ISharedLogic>(sharedLogic);
	    }
    }
}