using System;
using UnityEngine;
using UnityEngine.Assertions;
using PestelLib.TaskQueueLib;
using UnityEngine.SceneManagement;

namespace PestelLib.Quests
{ 
	public class Initializer : MonoBehaviour
	{
        private static TaskQueue _taskQueue = new TaskQueue() ;

	    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	    public static void InitOnLevelLoad()
	    {
	        if (SceneManager.GetActiveScene().name != "QuestsExampleScene")
	        {
	            return;
	        }

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
            /*var screen = MessageBoxScreen.ShowError("system_error_caption", "", null, null, null, null, null);
            screen.SetComplexDescription("system_error_init", exception.Message);*/
	    }

	    private static void TaskQueueOnOnQueueComplete()
	    {


            /*
	        MessageBoxScreen.Show(new MessageBoxDef
	        {
	            Caption = "Cap",
                Description = "Desc text",
                ButtonALabel = "A Button",
                ButtonBLabel = "B Button",
                ButtonAAction = () =>
                {
                    Debug.Log("Button a");
                },
                ButtonBAction = () =>
                {
                    Debug.Log("Button b");
                },
                AutoHide = true
	        });

            MessageBoxScreen.Show(new MessageBoxDef
            {
                Caption = "Cap",
                Description = "Second messagebox",
                ButtonALabel = "A Button",
                ButtonAAction = () =>
                {
                    Debug.Log("Button a");
                },
                AutoHide = true
            });
            */
        }
	}
}