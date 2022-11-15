using System;
using System.Collections;
using PestelLib.TaskQueueLib;
using PestelLib.Utils;
using UnityDI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;

namespace PestelLib.PremiumShop
{ 
	public class Initializer : MonoBehaviour
	{
        public static bool InitCompleted { get; private set; }

        private static TaskQueue _taskQueue = new TaskQueue() ;
	    public static Initializer Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init() {

            if (SceneManager.GetActiveScene().name != "PremiumShopExample")
                return;

		    Screen.sleepTimeout = SleepTimeout.NeverSleep;
		    Application.targetFrameRate = 30;
            Assert.raiseExceptions = true;

            _taskQueue.OnQueueComplete += TaskQueueOnOnQueueComplete;
            _taskQueue.OnQueueFailed += TaskQueueOnOnQueueFailed;
            
            _taskQueue.AddTask(new TaskInitDependencyInjection());
            _taskQueue.AddTask(new TaskInitScene());

            _taskQueue.RunQueue();
		}

	    private void OnDestroy()
	    {
            _taskQueue.OnQueueComplete -= TaskQueueOnOnQueueComplete;
            _taskQueue.OnQueueFailed -= TaskQueueOnOnQueueFailed;
	    }

	    private static void TaskQueueOnOnQueueFailed(Task task, Exception exception)
	    {
	        Debug.LogError("Initialization failed: " + exception.Message);
	    }

	    private static void TaskQueueOnOnQueueComplete()
	    {
	        InitCompleted = true;

	        var updateProvider = ContainerHolder.Container.Resolve<UpdateProvider>();
	        updateProvider.StartCoroutine(BuySomething());
	    }

	    public static IEnumerator BuySomething()
	    {
            yield return new WaitForSeconds(5);
	        var purchaser = ContainerHolder.Container.Resolve<Purchaser>();
            purchaser.BuyProductID("gcpack_1");
	    }
	}
}