namespace CameraScanner.Maui
{
    public class SMSResultParser : ResultParser
    {
        public override ParsedResult Parse(string source)
        {
            var text = source?.Trim();
            if (!string.IsNullOrEmpty(text) && text.StartsWith("sms:", StringComparison.OrdinalIgnoreCase))
            {
                return new SMSParsedResult(text.Substring("sms:".Length));
            }

            return null;
        }
    }
}