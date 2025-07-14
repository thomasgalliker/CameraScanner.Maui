namespace CameraScanner.Maui
{
    public sealed class ISBNResultParser : ResultParser
    {
        private static readonly char[] ISBNTrimChars = new[] { ' ', '\r', '\n' };

        /// <summary>
        /// Parses ISBN-13 barcode results (starting with 978 or 979).
        /// </summary>
        public override ParsedResult Parse(string source)
        {
            var rawText = GetMassagedText(source);
            var trimmed = rawText.Replace("-", "").Trim(ISBNTrimChars);
            if (trimmed.Length != 13)
            {
                return null;
            }

            if (!trimmed.StartsWith("978") && !trimmed.StartsWith("979"))
            {
                return null;
            }

            return new ISBNParsedResult(rawText);
        }
    }
}