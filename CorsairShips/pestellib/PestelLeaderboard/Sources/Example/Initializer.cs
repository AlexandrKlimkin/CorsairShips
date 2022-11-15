using System;
using UnityEngine;
using UnityEngine.Assertions;
using PestelLib.TaskQueueLib;

namespace PestelLib.Leaderboard
{ 
	public class Initializer : MonoBehaviour
	{
	    public event Action OnInitComplete = () => { };
        public bool InitCompleted { get; private set; }

        private TaskQueue _taskQueue = new TaskQueue() ;

	    public static Initializer Instance { get; private set; }

		void Start () {
		    if (Instance != null)
		    {
		        Destroy(gameObject);
                return;
		    }

            DontDestroyOnLoad(gameObject);

		    Screen.sleepTimeout = SleepTimeout.NeverSleep;
		    Application.targetFrameRate = 30;
            Assert.raiseExceptions = true;

            Instance = this;

            _taskQueue.OnQueueComplete += TaskQueueOnOnQueueComplete;
            _taskQueue.OnQueueFailed += TaskQueueOnOnQueueFailed;
            
            _taskQueue.AddTask(new TaskInitDependencyInjection());
            _taskQueue.AddTask(new TaskInitUserProfile());
            _taskQueue.AddTask(new TaskInitScene());

            _taskQueue.RunQueue();
		}

	    private void OnDestroy()
	    {
            _taskQueue.OnQueueComplete -= TaskQueueOnOnQueueComplete;
            _taskQueue.OnQueueFailed -= TaskQueueOnOnQueueFailed;
	    }

	    private void TaskQueueOnOnQueueFailed(Task task, Exception exception)
	    {
	        Debug.LogError("Initialization failed: " + exception.Message);
            /*var screen = MessageBoxScreen.ShowError("system_error_caption", "", null, null, null, null, null);
            screen.SetComplexDescription("system_error_init", exception.Message);*/
	    }

	    private void TaskQueueOnOnQueueComplete()
	    {
	        InitCompleted = true;
	        OnInitComplete();


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