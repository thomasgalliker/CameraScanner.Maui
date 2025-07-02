namespace CameraScanner.Maui
{
    public class EmailResultParser : ResultParser
    {
        public override ParsedResult Parse(string source)
        {
            var text = source?.Trim();
            if (!string.IsNullOrEmpty(text) && text.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                var email = text.Substring("mailto:".Length);
                var queryIndex = email.IndexOf('?');
                if (queryIndex >= 0)
                {
                    email = email.Substring(0, queryIndex);
                }

                return new EmailParsedResult(email);
            }

            return null;
        }
    }
}