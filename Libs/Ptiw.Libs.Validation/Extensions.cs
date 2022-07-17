namespace Ptiw.Libs.Validation
{
    public static class Extensions
    {
        public static bool ContainsAnyOf(this string str, List<string> array)
        {
            return array.Any(searchForItem => str.ToLower().Contains(searchForItem.ToLower()));
        }
    }
}