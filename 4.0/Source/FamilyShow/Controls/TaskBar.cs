using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Windows;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// This class is a helper class for the <see cref="Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager"/> type.
    /// </summary>
    internal class TaskBar
    {
        #region Properties

        private JumpList JumpList { get; set; }

        public static TaskBar Current { get; private set; }

        public bool IsPlatformSupported
        {
            get { return TaskbarManager.IsPlatformSupported; }
        }

        #endregion

        #region Initialization

        private TaskBar()
        {
        }

        private TaskBar(Window mainWindow, string appId)
        {
            TaskbarManager.Instance.ApplicationId = appId;
            JumpList = JumpList.CreateJumpListForIndividualWindow(TaskbarManager.Instance.ApplicationId, mainWindow);
            JumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Recent;
        }

        /// <summary>
        /// Creates a single task bar instance.
        /// </summary>
        /// <returns></returns>
        public static TaskBar Create(Window mainWindow, string appId, params IJumpListTask[] tasks)
        {
            if (Current == null)
            {
                if (TaskbarManager.IsPlatformSupported)
                {                    
                    Current = new TaskBar(mainWindow, appId);
                    Current.AddTasks(tasks);
                }
                else
                {
                    Current = new UnsupportedTaskBar();
                }
            }

            return Current;
        }

        #endregion

        #region Operations

        /// <summary>
        /// Creates a new taskbar jump list.
        /// </summary>
        /// <param name="mainWindow">The main window.</param>
        /// <param name="tasks">List of tasks that should be added to the jump list.</param>
        public virtual void AddTasks(params IJumpListTask[] tasks)
        {
            if (tasks == null || tasks.Length == 0)
            {
                return;
            }

            JumpList.AddUserTasks(tasks);
            JumpList.Refresh();
        }

        /// <summary>
        /// Set the progress of a Windows 7 taskbar.
        /// </summary>
        /// <param name="progress"></param>
        public virtual void Progress(int progress)
        {
            TaskbarManager.Instance.SetProgressValue((int)progress, 100);
        }

        /// <summary>
        /// Set the progress of a Windows 7 taskbar to indeterminate.
        /// </summary>
        public virtual void Loading()
        {
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate);
        }

        /// <summary>
        /// Clear the progress of a Windows 7 taskbar.
        /// </summary>
        public virtual void Restore()
        {
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
        }

        #endregion

        #region UnsupportedTaskBar Inner Type

        /// <summary>
        /// This type represents a dummy task-bar used in non-supported operating system.
        /// It simply ignores any call to the taskbar.
        /// </summary>
        private class UnsupportedTaskBar : TaskBar
        {
            public override void Loading() { }
            public override void Restore() { }
            public override void Progress(int progress) { }
            public override void AddTasks(params IJumpListTask[] tasks) { }
        }

        #endregion
    }
}
