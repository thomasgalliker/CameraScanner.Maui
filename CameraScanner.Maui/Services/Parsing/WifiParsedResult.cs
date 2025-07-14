using System.Text;

namespace CameraScanner.Maui
{
    public sealed class WifiParsedResult : ParsedResult
    {
        public WifiParsedResult(string networkEncryption, string ssid, string password)
            : this(networkEncryption, ssid, password, false)
        {
        }

        public WifiParsedResult(string networkEncryption, string ssid, string password, bool hidden)
        {
            this.Ssid = ssid;
            this.NetworkEncryption = networkEncryption;
            this.Password = password;
            this.Hidden = hidden;
        }

        public string Ssid { get; }

        public string NetworkEncryption { get; }

        public string Password { get; }

        public bool Hidden { get; }

        public override string GetDisplayResult()
        {
            var stringBuilder = new StringBuilder(80);
            MaybeAppend(this.Ssid, stringBuilder);
            MaybeAppend(this.NetworkEncryption, stringBuilder);
            MaybeAppend(this.Password, stringBuilder);
            MaybeAppend(this.Hidden.ToString(), stringBuilder);
            return stringBuilder.ToString();
        }

        private static void MaybeAppend(string value, StringBuilder result)
        {
            if (!string.IsNullOrEmpty(value))
            {
                result.AppendLine(value);
            }
        }
    }
}