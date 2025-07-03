namespace CameraScanner.Maui
{
    public class BarcodeParser : IBarcodeParser
    {
        private static readonly Lazy<BarcodeParser> Implementation =
            new Lazy<BarcodeParser>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

        /// <inheritdoc cref="IBarcodeParser.Current"/>
        public static BarcodeParser Current
        {
            get => Implementation.Value;
        }

        private static BarcodeParser CreateInstance()
        {
            return new BarcodeParser();
        }

        internal BarcodeParser()
        {
        }


        public ICollection<ResultParser> Parsers { get; } = new List<ResultParser>
        {
            new VCardResultParser(),
            new EmailResultParser(),
            new TelResultParser(),
            new SMSResultParser(),
            new GeoResultParser(),
            new WifiResultParser(),
            new ISBNResultParser(),
            new EmediplanParser(),
            new URIResultParser(),
        };

        public ParsedResult Parse(string source)
        {
            if (source != null)
            {
                foreach (var parser in this.Parsers)
                {
                    var parsedResult = parser.Parse(source);
                    if (parsedResult != null)
                    {
                        return parsedResult;
                    }
                }
            }

            return new TextParsedResult(source);
        }
    }
}