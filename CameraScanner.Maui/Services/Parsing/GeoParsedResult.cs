using System.Text;

namespace CameraScanner.Maui
{
    public sealed class GeoParsedResult : ParsedResult
    {
        public GeoParsedResult(double latitude, double longitude, double altitude, string query)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
            this.Query = query;
        }

        public string GeoURI
        {
            get
            {
                var result = new StringBuilder();
                result.Append("geo:");
                result.Append(this.Latitude);
                result.Append(',');
                result.Append(this.Longitude);
                if (this.Altitude > 0)
                {
                    result.Append(',');
                    result.Append(this.Altitude);
                }

                if (this.Query != null)
                {
                    result.Append('?');
                    result.Append(this.Query);
                }

                return result.ToString();
            }
        }

        public double Latitude { get; }

        public double Longitude { get; }

        public double Altitude { get; }

        public string Query { get; }

        public override string GetDisplayResult()
        {
            var result = new StringBuilder(20);
            result.Append(this.Latitude);
            result.Append(", ");
            result.Append(this.Longitude);
            if (this.Altitude > 0.0)
            {
                result.Append(", ");
                result.Append(this.Altitude);
                result.Append("m");
            }

            if (this.Query != null)
            {
                result.Append(" (");
                result.Append(this.Query);
                result.Append(')');
            }

            return result.ToString();
        }
    }
}