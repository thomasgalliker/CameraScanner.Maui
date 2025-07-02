namespace CameraScanner.Maui
{
    public class GeoParsedResult : ParsedResult
    {
        public string Geo { get; }

        public GeoParsedResult(string geo)
        {
            this.Geo = geo;
            this.DisplayResult = geo;
        }
    }
}