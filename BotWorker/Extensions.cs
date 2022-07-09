namespace Ptiw.HostApp
{
    internal static class Extensions
    {
        internal static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return (list == null) || !list.Any();
        }

        public static bool IsAllowedUser(this long id, IConfiguration configuration)
        {
            var allowedUsers = new List<long> { long.Parse(configuration["Wife_ID"]), long.Parse(configuration["Husband_ID"]) };
            return allowedUsers.Contains(id);
        }
    }
}