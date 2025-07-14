namespace CameraScanner.Maui
{
    public static class IBarcodeParserExtensions
    {
        public static TParsedResult Parse<TParsedResult>(this IBarcodeParser barcodeParser, string source) where TParsedResult : ParsedResult
        {
            var result = barcodeParser.Parse(source);
            if (result is not TParsedResult parsedResult)
            {
                throw new NotSupportedException($"Failed to parse to target ParsedResult '{typeof(TParsedResult).Name}'");
            }

            return parsedResult;
        }

    }
}