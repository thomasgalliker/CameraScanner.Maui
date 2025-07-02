namespace CameraScanner.Maui
{
    public class SMSParsedResult : ParsedResult
    {
        public string SMS { get; }

        public SMSParsedResult(string sms)
        {
            this.SMS = sms;
            this.DisplayResult = sms;
        }
    }
}