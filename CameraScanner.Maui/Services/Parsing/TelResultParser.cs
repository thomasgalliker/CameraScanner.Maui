namespace CameraScanner.Maui
{
    public class TelResultParser : ResultParser
    {
        public override ParsedResult Parse(string source)
        {
            var text = source?.Trim();
            if (!string.IsNullOrEmpty(text) && text.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
            {
                return new TelParsedResult(text.Substring("tel:".Length));
            }

            return null;
        }
    }
}