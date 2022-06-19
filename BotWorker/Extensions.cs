using Quartz;

namespace Ptiw.HostApp
{
    internal static class Extensions
    {
        internal static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return (list == null) || !list.Any();
        }

        public static string GetTaskData(this IJob job, IConfiguration configuration, string settingName, string group = null)
        {
            var jobName = job.GetType().Name;
            var settingPath = group == null ? $"Tasks:{jobName}:{settingName}" : $"Tasks:{jobName}:{group}:{settingName}";
            return configuration[settingPath];
        }

        public static bool IsAllowedUser(this long id, IConfiguration configuration)
        {
            var allowedUsers = new List<long> { long.Parse(configuration["Wife_ID"]), long.Parse(configuration["Husband_ID"]) };
            return allowedUsers.Contains(id);
        }
    }
}