using System.Text;
using CameraScanner.Maui.Extensions;

namespace CameraScanner.Maui
{
    public sealed class EmailParsedResult : ParsedResult
    {
        public EmailParsedResult(string[] tos, string[] ccs, string[] bccs, string subject, string body)
        {
            this.Tos = tos;
            this.CCs = ccs;
            this.BCCs = bccs;
            this.Subject = subject;
            this.Body = body;
        }

        public string[] Tos { get; }

        public string[] CCs { get; }

        public string[] BCCs { get; }

        public string Subject { get; }

        public string Body { get; }

        public override string GetDisplayResult()
        {
            var result = new StringBuilder();
            result.AppendIfNotNullOrEmpty(this.Tos);
            result.AppendIfNotNullOrEmpty(this.CCs);
            result.AppendIfNotNullOrEmpty(this.BCCs);
            result.AppendIfNotNullOrEmpty(this.Subject);
            result.AppendIfNotNullOrEmpty(this.Body);
            return result.ToString();
        }
    }
}