using System.Text.RegularExpressions;

namespace CameraScanner.Maui
{
    public sealed class EmailResultParser : ResultParser
    {
        private static readonly Regex COMMA = new Regex(",", RegexOptions.Compiled);
        private const string MailtoPrefix = "mailto:";

        public override ParsedResult Parse(string result)
        {
            var rawText = GetMassagedText(result);
            if (!rawText.StartsWith(MailtoPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var hostEmail = rawText[MailtoPrefix.Length..];
            var queryStart = hostEmail.IndexOf('?');
            if (queryStart >= 0)
            {
                hostEmail = hostEmail[..queryStart];
            }

            try
            {
                hostEmail = UrlDecode(hostEmail);
            }
            catch (ArgumentException)
            {
                return null;
            }

            string[] tos = null;
            if (!string.IsNullOrEmpty(hostEmail))
            {
                tos = COMMA.Split(hostEmail);
            }

            var nameValues = ParseNameValuePairs(rawText);
            string[] ccs = null;
            string[] bccs = null;
            string subject = null;
            string body = null;

            if (nameValues != null)
            {
                if (tos == null && nameValues.TryGetValue("to", out var tosString) && tosString != null)
                {
                    tos = COMMA.Split(tosString);
                }

                if (nameValues.TryGetValue("cc", out var ccString) && ccString != null)
                {
                    ccs = COMMA.Split(ccString);
                }

                if (nameValues.TryGetValue("bcc", out var bccString) && bccString != null)
                {
                    bccs = COMMA.Split(bccString);
                }

                nameValues.TryGetValue("subject", out subject);
                nameValues.TryGetValue("body", out body);
            }

            return new EmailParsedResult(tos, ccs, bccs, subject, body);

        }
    }
}