namespace CameraScanner.Maui
{
    public class URIResultParser : ResultParser
    {
        public override ParsedResult Parse(string source)
        {
            var text = source?.Trim();
            if (Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out var uri))
            {
                return new URIParsedResult(text, uri);
            }

            return null;
        }
    }
}