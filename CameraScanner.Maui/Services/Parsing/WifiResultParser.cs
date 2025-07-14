namespace CameraScanner.Maui
{
    public class WifiResultParser : ResultParser
    {
        private const string OpenNetworkType = "nopass";

        public override ParsedResult Parse(string source)
        {
            var rawText = GetMassagedText(source);
            if (!rawText.StartsWith("WIFI:"))
            {
                return null;
            }

            var ssid = MatchSinglePrefixedField("S:", rawText, ';', false);
            if (ssid == null || string.IsNullOrEmpty(ssid))
            {
                return null;
            }

            var pass = MatchSinglePrefixedField("P:", rawText, ';', false);
            var type = MatchSinglePrefixedField("T:", rawText, ';', false);
            if (type == null)
            {
                type = OpenNetworkType;
            }

            bool.TryParse(MatchSinglePrefixedField("H:", rawText, ';', false), out var hidden);
            return new WifiParsedResult(type, ssid, pass, hidden);
        }
    }
}