using System;
using TaskScheduler;

namespace lockit
{
    internal class TaskSched
    {
        public static void CreateUACBypass(string PathToExe, string args, string name)
        {
            ITaskService taskService = new TaskScheduler.TaskScheduler();
            taskService.Connect();
            ITaskDefinition taskDefinition = taskService.NewTask(0);
            taskDefinition.Settings.Enabled = true;
            taskDefinition.Settings.Compatibility = _TASK_COMPATIBILITY.TASK_COMPATIBILITY_V2_1;
            taskDefinition.Settings.RunOnlyIfIdle = false;
            taskDefinition.Settings.AllowDemandStart = true;
            taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            taskDefinition.Settings.StopIfGoingOnBatteries = false;
            taskDefinition.Principal.LogonType = _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN;
            taskDefinition.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;

            IActionCollection actions = taskDefinition.Actions;
            _TASK_ACTION_TYPE actionType = _TASK_ACTION_TYPE.TASK_ACTION_EXEC;

            //create new action
            IAction action = actions.Create(actionType);
            IExecAction execAction = action as IExecAction;
            execAction.Path = @PathToExe;
            execAction.WorkingDirectory = @AppDomain.CurrentDomain.BaseDirectory;
            execAction.Arguments = @args;
            ITaskFolder rootFolder = taskService.GetFolder(@"\");

            //register task.
            rootFolder.RegisterTaskDefinition(@name, taskDefinition, 6, null, null, _TASK_LOGON_TYPE.TASK_LOGON_NONE, null);
        }

        public static void RemoveUACBypass()
        {
            ITaskService taskService = new TaskScheduler.TaskScheduler();
            taskService.Connect();
            ITaskFolder taskFolder = taskService.GetFolder("");

            try
            {
                taskFolder.DeleteTask("LockitUACBypass", 0);
                taskFolder.DeleteTask("LockitUACBypassSetup", 0);
            }
            catch (Exception) { }
        }

        public static bool IsUACbypassEnabled()
        {
            ITaskService taskService = new TaskScheduler.TaskScheduler();
            taskService.Connect();
            ITaskFolder taskFolder = taskService.GetFolder("");
            try
            {
                taskFolder.GetTask("LockitUACBypass");
                taskFolder.GetTask("LockitUACBypassSetup");
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
