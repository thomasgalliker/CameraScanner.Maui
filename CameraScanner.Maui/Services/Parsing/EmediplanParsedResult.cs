namespace CameraScanner.Maui
{
    public class EmediplanParsedResult : ParsedResult
    {
        public EmediplanParsedResult(string rawText, string releaseYear, string subVersion, string additionalMetaData, string data)
        {
            this.RawText = rawText;
            this.ReleaseYear = releaseYear;
            this.SubVersion = subVersion;
            this.AdditionalMetaData = additionalMetaData;
            this.Data = data;
        }

        public string RawText { get; }

        public string ReleaseYear { get; }

        public string SubVersion { get; }

        public string AdditionalMetaData { get; }

        public string Data { get; }
    }
}