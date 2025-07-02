namespace CameraScanner.Maui
{
    public class TelParsedResult : ParsedResult
    {
        public string Number { get; }

        public TelParsedResult(string number)
        {
            this.Number = number;
            this.DisplayResult = number;
        }
    }
}