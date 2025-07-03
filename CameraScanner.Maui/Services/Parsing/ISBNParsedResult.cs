namespace CameraScanner.Maui
{
    public sealed class ISBNParsedResult : ParsedResult
    {
        public ISBNParsedResult(string isbn)
        {
            this.ISBN = isbn;
            this.DisplayResult = isbn;
        }

        public string ISBN { get; }

        public override string GetDisplayResult()
        {
            return this.ISBN;
        }
    }
}