namespace CameraScanner.Maui
{
    public class TextParsedResult : ParsedResult
    {
        public string Text { get; }

        public TextParsedResult(string text)
        {
            this.Text = text;
            this.DisplayResult = text;
        }
    }
}