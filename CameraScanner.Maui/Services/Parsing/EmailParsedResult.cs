namespace CameraScanner.Maui
{
    public class EmailParsedResult : ParsedResult
    {
        public string Email { get; }

        public EmailParsedResult(string email)
        {
            this.Email = email;
            this.DisplayResult = email;
        }
    }
}