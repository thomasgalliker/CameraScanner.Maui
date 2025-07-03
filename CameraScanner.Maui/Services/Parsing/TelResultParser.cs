namespace CameraScanner.Maui
{
    public sealed class TelResultParser : ResultParser
    {
        private const string TelPrefix = "tel:";

        public override ParsedResult Parse(string source)
        {
            var rawText = GetMassagedText(source);
            if (!rawText.StartsWith(TelPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            // Normalize "TEL:" to "tel:"
            var telPrefixLength = TelPrefix.Length;
            var telUri = rawText.StartsWith(TelPrefix, StringComparison.InvariantCultureIgnoreCase)
                ? TelPrefix + rawText[telPrefixLength..]
                : rawText;

            // Drop tel: and optional query portion
            var queryStart = rawText.IndexOf('?', telPrefixLength);
            var number = queryStart < 0 ? rawText[telPrefixLength..] : rawText.Substring(telPrefixLength, queryStart - telPrefixLength);

            return new TelParsedResult(number, new Uri(telUri));
        }
    }
}