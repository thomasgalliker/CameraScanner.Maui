namespace CameraScanner.Maui
{
    public class GeoResultParser : ResultParser
    {
        public override ParsedResult Parse(string source)
        {
            var text = source?.Trim();
            if (!string.IsNullOrEmpty(text) && text.StartsWith("geo:", StringComparison.OrdinalIgnoreCase))
            {
                return new GeoParsedResult(text);
            }

            return null;
        }
    }
}