using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskScheduler;
using System.Diagnostics;

namespace FakeISCT
{
    class TaskSchedulerManager
    {
        private ITaskService scheduler;
        private ITaskFolder rootFolder;
        private ITaskDefinition task;
        private IExecAction action;
        private ITimeTrigger trigger;
        private EventLog eventLog;

        public TaskSchedulerManager(EventLog el)
        {
            scheduler = new TaskScheduler.TaskScheduler();
            scheduler.Connect(null, null, null, null);
            rootFolder = scheduler.GetFolder("\\");
            eventLog = el;
        }

        public void addWakeUpTask(int sT)
        {
            //rootFolder.CreateFolder("", &newFolder);

            task = scheduler.NewTask(0);
            task.Settings.Enabled = true;
            task.Settings.WakeToRun = true;
            task.Settings.StopIfGoingOnBatteries = false;
            task.Settings.RunOnlyIfIdle = false;
            task.Settings.DisallowStartIfOnBatteries = false;
            task.Settings.IdleSettings.StopOnIdleEnd = false;
            task.Settings.RestartCount = 5;
            task.Settings.RestartInterval = "PT1M";
            task.Settings.StartWhenAvailable = true;
            task.Settings.Priority = 0;
            task.Settings.MultipleInstances = _TASK_INSTANCES_POLICY.TASK_INSTANCES_PARALLEL;
            task.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;
            task.Principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_SERVICE_ACCOUNT;
            task.Principal.UserId = "System";

            action = (IExecAction)task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);

            //action.Path = typeof(SayHello.Form1).Assembly.Location;
            action.Path = @"c:\windows\notepad.exe";
            //action.Path = @"C:\Users\sagatsum\Documents\Visual Studio 2010\Projects\FakeISCTConsole\FakeISCTConsole\bin\Release\FakeISCTConsole.exe";
            //action.WorkingDirectory = Path.GetDirectoryName(typeof(SayHello.Form1).Assembly.Location);
            //action.WorkingDirectory = @"C:\Users\sagatsum\Documents\Visual Studio 2010\Projects\FakeISCTConsole\FakeISCTConsole\bin\Release";

            trigger = (ITimeTrigger)task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME);
            //trigger.StateChange = _TASK_SESSION_STATE_CHANGE_TYPE.TASK_SESSION_UNLOCK;
            trigger.Enabled = true;
            trigger.StartBoundary = (DateTime.Now.AddSeconds(sT)).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");

            task.RegistrationInfo.Author = "Shintaro Agatsuma";
            task.RegistrationInfo.Description = "Fake iSCT Task.";
            try
            {
                IRegisteredTask ticket = rootFolder.RegisterTaskDefinition("Fake iSCT Wake Up", task,
                    (int)_TASK_CREATION.TASK_CREATE_OR_UPDATE, null, null, _TASK_LOGON_TYPE.TASK_LOGON_S4U, null);
                eventLog.WriteEntry("Task Successfully added");
            }
            catch (Exception e)
            {
                eventLog.WriteEntry("Failed to add a task: " + e.Message);
            }
        }
    }
}
