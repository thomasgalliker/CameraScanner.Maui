using System.Text.RegularExpressions;

namespace CameraScanner.Maui
{
    public sealed class GeoResultParser : ResultParser
    {
        private static readonly Regex GeoUrlPattern = new Regex(@"geo:([\-0-9.]+),([\-0-9.]+)(?:,([\-0-9.]+))?(?:\?(.*))?", RegexOptions.IgnoreCase);

        public override ParsedResult Parse(string source)
        {
            var rawText = GetMassagedText(source);
            var match = GeoUrlPattern.Match(rawText);
            if (!match.Success)
            {
                return null;
            }

            var query = match.Groups[4].Value;

            try
            {
                var latitude = double.Parse(match.Groups[1].Value);
                if (latitude is > 90.0 or < -90.0)
                {
                    return null;
                }

                var longitude = double.Parse(match.Groups[2].Value);
                if (longitude is > 180.0 or < -180.0)
                {
                    return null;
                }

                var altitude = 0.0;
                if (match.Groups[3].Success)
                {
                    altitude = double.Parse(match.Groups[3].Value);
                    if (altitude < 0.0)
                    {
                        return null;
                    }
                }

                return new GeoParsedResult(latitude, longitude, altitude, query);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}