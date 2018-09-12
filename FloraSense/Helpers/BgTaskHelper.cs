using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using FloraBackground;
using FloraSense.Helpers;

namespace FloraSense
{
    public class BgTaskHelper
    {
        private static string TaskName { get; } = typeof(FloraBackgroundTask).Name;
        private static string TaskEntryPoint { get; } = typeof(FloraBackgroundTask).FullName;

        public static bool TaskRegistered => GetTask() != null;

        public static async Task<bool> RegisterTask(uint period)
        {
            if (BackgroundTaskRegistration.AllTasks.Any(pair => pair.Value.Name == TaskName))
                return false;

            var access = await BackgroundExecutionManager.RequestAccessAsync();

            if (access != BackgroundAccessStatus.AlwaysAllowed &&
                access != BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
            {
                await Helpers.Helpers.ShowMessage("Insufficient permissions to schedule a background task");
                return false;
            }

            var builder = new BackgroundTaskBuilder
            {
                Name = TaskName,
                TaskEntryPoint = TaskEntryPoint
            };
            builder.SetTrigger(new TimeTrigger(15, false));
            builder.Register();
            return true;
        }

        public static void UnregisterTask()
        {
            GetTask()?.Unregister(false);
        }

        public static IBackgroundTaskRegistration GetTask()
        {
            return BackgroundTaskRegistration.AllTasks.FirstOrDefault(p => p.Value.Name == TaskName).Value;
        }
    }
}
