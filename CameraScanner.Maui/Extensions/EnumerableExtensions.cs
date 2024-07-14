namespace CameraScanner.Maui
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<string> SplitToLines(this string input)
        {
            if (input == null)
            {
                yield break;
            }

            using (var reader = new StringReader(input))
            {
                while (reader.ReadLine() is string line)
                {
                    yield return line;
                }
            }
        }
    }
}