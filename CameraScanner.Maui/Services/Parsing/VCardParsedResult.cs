namespace CameraScanner.Maui
{
    public class VCardParsedResult : ParsedResult
    {
        public string VCardText { get; }

        public VCardParsedResult(string text)
        {
            this.VCardText = text;
            this.DisplayResult = text;
        }
    }
}