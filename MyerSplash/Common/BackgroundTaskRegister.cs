﻿using BackgroundTask;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace MyerSplash.Common
{
    public static class BackgroundTaskRegister
    {
        private static string NAME => "WallpaperAutoChangeTask";
        private static uint PERIOD_MINS => 240; // 4 hours

        public static async Task RegisterAsync()
        {
            if (IsBackgroundTaskRegistered())
            {
                Debug.WriteLine("IsBackgroundTaskRegistered: true");
                return;
            }
            await RegisterBackgroundTask(typeof(WallpaperAutoChangeTask),
                                                    new TimeTrigger(PERIOD_MINS, false),
                                                    null);
        }

        public static async Task UnregisterAsync()
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status != BackgroundAccessStatus.AlwaysAllowed
                && status != BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
            {
                return;
            }

            foreach (var (_, value) in BackgroundTaskRegistration.AllTasks)
            {
                if (value.Name == NAME)
                {
                    value.Unregister(true);
                }
            }

            Debug.WriteLine($"===================unregistered===================");
        }

        private static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(Type taskEntryPoint,
                                                                IBackgroundTrigger trigger,
                                                                IBackgroundCondition condition)
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status != BackgroundAccessStatus.AlwaysAllowed
                && status != BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
            {
                return null;
            }

            foreach (var (_, value) in BackgroundTaskRegistration.AllTasks)
            {
                if (value.Name == NAME)
                {
                    value.Unregister(true);
                }
            }

            var builder = new BackgroundTaskBuilder
            {
                Name = NAME,
                TaskEntryPoint = taskEntryPoint.FullName
            };

            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);
            }

            var task = builder.Register();

            Debug.WriteLine($"===================Task {NAME} registered successfully===================");

            return task;
        }

        private static bool IsBackgroundTaskRegistered()
        {
            return BackgroundTaskRegistration.AllTasks.Any(task => task.Value.Name == NAME);
        }
    }
}