using System.Text;
using CameraScanner.Maui.Extensions;

namespace CameraScanner.Maui
{
    public sealed class TelParsedResult : ParsedResult
    {
        public string Number { get; }

        public Uri TelUri { get; }

        public TelParsedResult(string number, Uri telUri)
        {
            this.Number = number;
            this.TelUri = telUri;
        }

        public override string GetDisplayResult()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendIfNotNullOrEmpty(this.Number);
            return stringBuilder.ToString();
        }
    }
}