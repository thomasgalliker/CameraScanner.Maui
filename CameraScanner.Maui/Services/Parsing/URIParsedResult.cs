namespace CameraScanner.Maui
{
    public class URIParsedResult : ParsedResult
    {
        public Uri Uri { get; }

        public URIParsedResult(string source, Uri uri)
        {
            this.DisplayResult = source;
            this.Uri = uri;
        }
    }
}