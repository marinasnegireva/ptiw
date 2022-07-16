namespace Ptiw.Libs.Validation
{
    internal static class Extensions
    {
        internal static bool ContainsAnyOf(this string str, List<string> array)
        {
            return array.Any(searchForItem => str.ToLower().Contains(searchForItem.ToLower()));
        }
    }
}