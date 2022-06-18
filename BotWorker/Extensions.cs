using Quartz;
using System.Collections.Generic;
using System.Linq;

namespace Ptiw.HostApp
{
    internal static class Extensions
    {
        internal static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return (list == null) || !list.Any();
        }

        public static string GetTaskData(this IJob job, string settingName, string group = null)
        {
            var jobName = job.GetType().Name;
            var dataJObject = Settings.SettingsManager.AppSettings.Tasks.ContainsKey(jobName) ? Settings.SettingsManager.AppSettings.Tasks[jobName] : null;
            if (dataJObject == null)
            {
                throw new KeyNotFoundException($"Job {jobName} is not configured");
            }
            return dataJObject == null ? null : group != null ? (string)dataJObject?[group]?[settingName] : (string)dataJObject?[settingName];
        }
    }
}